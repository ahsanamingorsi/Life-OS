using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LifeOS.App.Services.Sync
{
    /// <summary>Shared pieces of the phone-PC sync: pairing code, key, handshake and encrypted messages.</summary>
    public static class SyncProtocol
    {
        public const int TcpPort = 5198;
        public const int DiscoveryPort = 5199;
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("LifeOS-sync-v1");

        /// <summary>A random pairing code like K7M2-9QXA-4TWD (60 bits).</summary>
        public static string NewCode()
        {
            var chars = new char[12];
            for (int i = 0; i < chars.Length; i++) chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
            var s = new string(chars);
            return $"{s[..4]}-{s[4..8]}-{s[8..]}";
        }

        public static string NormalizeCode(string? code) =>
            new((code ?? "").ToUpperInvariant().Where(c => Alphabet.Contains(c)).ToArray());

        public static bool IsValidCode(string? code) => NormalizeCode(code).Length == 12;

        public static byte[] DeriveKey(string code) =>
            Rfc2898DeriveBytes.Pbkdf2(NormalizeCode(code), Salt, 100_000, HashAlgorithmName.SHA256, 32);

        private static byte[] Hmac(byte[] key, string label, byte[] a, byte[] b)
        {
            using var h = new HMACSHA256(key);
            return h.ComputeHash(Encoding.ASCII.GetBytes(label).Concat(a).Concat(b).ToArray());
        }

        // ── raw frames (length + bytes) ───────────────────────────────────
        public static async Task WriteFrameAsync(Stream s, byte[] data, CancellationToken ct)
        {
            var len = BitConverter.GetBytes(data.Length);
            await s.WriteAsync(len, ct);
            await s.WriteAsync(data, ct);
            await s.FlushAsync(ct);
        }

        public static async Task<byte[]> ReadFrameAsync(Stream s, CancellationToken ct, int max = 256 * 1024 * 1024)
        {
            var lenBuf = new byte[4];
            await ReadExactAsync(s, lenBuf, ct);
            int len = BitConverter.ToInt32(lenBuf);
            if (len < 0 || len > max) throw new InvalidDataException("Bad message size.");
            var data = new byte[len];
            await ReadExactAsync(s, data, ct);
            return data;
        }

        private static async Task ReadExactAsync(Stream s, byte[] buf, CancellationToken ct)
        {
            int read = 0;
            while (read < buf.Length)
            {
                int n = await s.ReadAsync(buf.AsMemory(read), ct);
                if (n == 0) throw new EndOfStreamException("Connection closed.");
                read += n;
            }
        }

        // ── handshake: both sides prove they know the code, then derive a session key ──
        public static async Task<SecureChannel> ClientHandshakeAsync(Stream s, string code, CancellationToken ct)
        {
            var key = DeriveKey(code);
            var nonceC = RandomNumberGenerator.GetBytes(16);
            await WriteFrameAsync(s, Encoding.ASCII.GetBytes("LOS1").Concat(nonceC).ToArray(), ct);

            var reply = await ReadFrameAsync(s, ct, 1024);
            if (reply.Length != 48) throw new InvalidDataException("Unexpected reply.");
            var nonceS = reply[..16];
            if (!CryptographicOperations.FixedTimeEquals(reply[16..], Hmac(key, "S", nonceC, nonceS)))
                throw new UnauthorizedAccessException("Wrong code (or this is not your PC).");

            await WriteFrameAsync(s, Hmac(key, "C", nonceS, nonceC), ct);
            return new SecureChannel(s, Hmac(key, "K", nonceC, nonceS), isClient: true);
        }

        public static async Task<SecureChannel> ServerHandshakeAsync(Stream s, string code, CancellationToken ct)
        {
            var key = DeriveKey(code);
            var hello = await ReadFrameAsync(s, ct, 1024);
            if (hello.Length != 20 || Encoding.ASCII.GetString(hello, 0, 4) != "LOS1") throw new InvalidDataException("Not a Life OS client.");
            var nonceC = hello[4..];
            var nonceS = RandomNumberGenerator.GetBytes(16);
            await WriteFrameAsync(s, nonceS.Concat(Hmac(key, "S", nonceC, nonceS)).ToArray(), ct);

            var proof = await ReadFrameAsync(s, ct, 1024);
            if (!CryptographicOperations.FixedTimeEquals(proof, Hmac(key, "C", nonceS, nonceC)))
            {
                await Task.Delay(1500, ct); // slow down guessing
                throw new UnauthorizedAccessException("Wrong code.");
            }
            return new SecureChannel(s, Hmac(key, "K", nonceC, nonceS), isClient: false);
        }
    }

    /// <summary>Encrypted, ordered messages (AES-GCM, counter nonces).</summary>
    public sealed class SecureChannel
    {
        private readonly Stream _s;
        private readonly AesGcm _aes;
        private readonly byte _sendDir, _recvDir;
        private ulong _sendCount, _recvCount;

        public SecureChannel(Stream s, byte[] key, bool isClient)
        {
            _s = s;
            _aes = new AesGcm(key, 16);
            _sendDir = (byte)(isClient ? 1 : 2);
            _recvDir = (byte)(isClient ? 2 : 1);
        }

        private static byte[] Nonce(byte dir, ulong n)
        {
            var nonce = new byte[12];
            nonce[0] = dir;
            BitConverter.GetBytes(n).CopyTo(nonce, 4);
            return nonce;
        }

        public async Task SendAsync(byte[] plain, CancellationToken ct)
        {
            var nonce = Nonce(_sendDir, _sendCount++);
            var cipher = new byte[plain.Length];
            var tag = new byte[16];
            _aes.Encrypt(nonce, plain, cipher, tag);
            await SyncProtocol.WriteFrameAsync(_s, cipher.Concat(tag).ToArray(), ct);
        }

        public async Task<byte[]> ReceiveAsync(CancellationToken ct)
        {
            var frame = await SyncProtocol.ReadFrameAsync(_s, ct);
            if (frame.Length < 16) throw new InvalidDataException("Short message.");
            var nonce = Nonce(_recvDir, _recvCount++);
            var plain = new byte[frame.Length - 16];
            _aes.Decrypt(nonce, frame.AsSpan(0, plain.Length), frame.AsSpan(plain.Length, 16), plain);
            return plain;
        }
    }

    /// <summary>Finds the PC on the same Wi-Fi with a UDP broadcast.</summary>
    public static class SyncDiscovery
    {
        public static async Task<string?> FindPcAsync(int timeoutMs = 2500)
        {
            try
            {
                using var udp = new UdpClient { EnableBroadcast = true };
                udp.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                var msg = Encoding.ASCII.GetBytes("LIFEOS-DISCOVER");
                await udp.SendAsync(msg, msg.Length, new IPEndPoint(IPAddress.Broadcast, SyncProtocol.DiscoveryPort));
                using var cts = new CancellationTokenSource(timeoutMs);
                while (!cts.IsCancellationRequested)
                {
                    var res = await udp.ReceiveAsync(cts.Token);
                    if (Encoding.ASCII.GetString(res.Buffer).StartsWith("LIFEOS-HERE")) return res.RemoteEndPoint.Address.ToString();
                }
            }
            catch { }
            return null;
        }

        /// <summary>This PC's Wi-Fi / LAN addresses, to show next to the code.</summary>
        public static List<string> LocalAddresses() =>
            System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                            && n.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .Where(a => !a.StartsWith("169.254."))
                .ToList();
    }
}
