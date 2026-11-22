using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using LifeOS.MVC.Data;
using LifeOS.MVC.Sync;

namespace LifeOS.App.Services.Sync
{
    internal static class SyncPayload
    {
        public static byte[] Pack(JsonObject snapshot)
        {
            using var ms = new MemoryStream();
            using (var gz = new GZipStream(ms, CompressionLevel.Fastest, leaveOpen: true))
                gz.Write(Encoding.UTF8.GetBytes(snapshot.ToJsonString()));
            return ms.ToArray();
        }

        public static JsonObject Unpack(byte[] data)
        {
            using var gz = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
            using var reader = new StreamReader(gz, Encoding.UTF8);
            return JsonNode.Parse(reader.ReadToEnd())!.AsObject();
        }
    }

    /// <summary>PC side: waits for the phone on the local network. Only runs when switched on in Settings.</summary>
    public sealed class SyncServer : IDisposable
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly AppUi _ui;
        private readonly SemaphoreSlim _one = new(1, 1);
        private TcpListener? _tcp;
        private UdpClient? _udp;
        private CancellationTokenSource? _cts;

        public SyncServer(IServiceScopeFactory scopes, AppUi ui) { _scopes = scopes; _ui = ui; }

        public bool IsRunning { get; private set; }
        public string? LastMessage { get; private set; }
        public DateTime? LastSyncAt { get; private set; }
        public event Action? Changed;

        public async Task<string> GetCodeAsync()
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var code = await SettingsStore.GetAsync(db, "sync.code", "");
            if (!SyncProtocol.IsValidCode(code))
            {
                code = SyncProtocol.NewCode();
                await SettingsStore.SetAsync(db, "sync.code", code);
            }
            return code;
        }

        /// <summary>A new code: every paired phone has to enter it again.</summary>
        public async Task<string> NewCodeAsync()
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var code = SyncProtocol.NewCode();
            await SettingsStore.SetAsync(db, "sync.code", code);
            return code;
        }

        public async Task StartIfEnabledAsync()
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (await SettingsStore.GetAsync(db, "sync.server", "0") == "1") await StartAsync();
        }

        public async Task StartAsync()
        {
            if (IsRunning) return;
            await GetCodeAsync();
            _cts = new CancellationTokenSource();
            try
            {
                _tcp = new TcpListener(IPAddress.Any, SyncProtocol.TcpPort);
                _tcp.Start();
                _udp = new UdpClient(new IPEndPoint(IPAddress.Any, SyncProtocol.DiscoveryPort)) { EnableBroadcast = true };
            }
            catch (Exception ex)
            {
                LastMessage = "Could not start sync: " + ex.Message;
                _tcp?.Stop(); _udp?.Dispose(); _tcp = null; _udp = null;
                Changed?.Invoke();
                return;
            }
            IsRunning = true;
            LastMessage = "Waiting for your phone…";
            var token = _cts.Token;
            _ = Task.Run(() => AcceptLoopAsync(token));
            _ = Task.Run(() => DiscoveryLoopAsync(token));
            Changed?.Invoke();
        }

        public void Stop()
        {
            _cts?.Cancel();
            _tcp?.Stop(); _udp?.Dispose();
            _tcp = null; _udp = null;
            IsRunning = false;
            Changed?.Invoke();
        }

        private async Task DiscoveryLoopAsync(CancellationToken ct)
        {
            try
            {
                var reply = Encoding.ASCII.GetBytes("LIFEOS-HERE");
                while (!ct.IsCancellationRequested)
                {
                    var res = await _udp!.ReceiveAsync(ct);
                    if (Encoding.ASCII.GetString(res.Buffer) == "LIFEOS-DISCOVER")
                        await _udp.SendAsync(reply, reply.Length, res.RemoteEndPoint);
                }
            }
            catch { }
        }

        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var client = await _tcp!.AcceptTcpClientAsync(ct);
                    _ = Task.Run(() => HandleAsync(client, ct));
                }
            }
            catch { }
        }

        private async Task HandleAsync(TcpClient client, CancellationToken serverToken)
        {
            await _one.WaitAsync(serverToken);
            try
            {
                using (client)
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(serverToken);
                    cts.CancelAfter(TimeSpan.FromMinutes(3));
                    var ct = cts.Token;
                    using var stream = client.GetStream();

                    var chan = await SyncProtocol.ServerHandshakeAsync(stream, await GetCodeAsync(), ct);
                    var theirs = SyncPayload.Unpack(await chan.ReceiveAsync(ct));

                    SyncStats stats;
                    JsonObject mine;
                    using (var scope = _scopes.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        await SyncEngine.EnsureStampsAsync(db);
                        stats = await SyncEngine.MergeAsync(db, theirs);
                        mine = await SyncEngine.ExportAsync(db);
                    }
                    await chan.SendAsync(SyncPayload.Pack(mine), ct);

                    LastSyncAt = DateTime.Now;
                    LastMessage = $"Phone synced at {LastSyncAt:HH:mm} ({stats})";
                    if (stats.Total > 0) _ui.NotifyDataChanged();
                    Changed?.Invoke();
                }
            }
            catch (Exception ex)
            {
                LastMessage = "A sync attempt failed: " + ex.Message;
                Changed?.Invoke();
            }
            finally { _one.Release(); }
        }

        public void Dispose() => Stop();
    }

    /// <summary>Phone side (works on PC too): connects to the PC and exchanges changes.</summary>
    public sealed class SyncClient : IDisposable
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly AppUi _ui;
        private readonly SemaphoreSlim _one = new(1, 1);
        private System.Threading.Timer? _timer;

        public SyncClient(IServiceScopeFactory scopes, AppUi ui) { _scopes = scopes; _ui = ui; }

        public bool Busy { get; private set; }
        public string? LastMessage { get; private set; }
        public DateTime? LastSyncAt { get; private set; }
        public event Action? Changed;

        /// <summary>Syncs once now, then again every 10 minutes while the app is open (only if a PC is saved).</summary>
        public void StartAuto() =>
            _timer ??= new System.Threading.Timer(_ => _ = SyncNowAsync(silent: true), null, TimeSpan.FromSeconds(8), TimeSpan.FromMinutes(10));

        public async Task<(string? Host, string? Code)> GetPeerAsync()
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var host = await SettingsStore.GetAsync(db, "sync.peer", "");
            var code = await SettingsStore.GetAsync(db, "sync.peerCode", "");
            return (host.Length == 0 ? null : host, code.Length == 0 ? null : code);
        }

        public async Task SavePeerAsync(string host, string code)
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SettingsStore.SetAsync(db, "sync.peer", host.Trim());
            await SettingsStore.SetAsync(db, "sync.peerCode", SyncProtocol.NormalizeCode(code));
        }

        public async Task ForgetPeerAsync()
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await SettingsStore.SetAsync(db, "sync.peer", "");
            await SettingsStore.SetAsync(db, "sync.peerCode", "");
        }

        public async Task<(bool Ok, string Message)> SyncNowAsync(bool silent = false)
        {
            var (host, code) = await GetPeerAsync();
            if (host == null || code == null) return (false, "No PC saved yet.");
            if (!await _one.WaitAsync(0)) return (false, "A sync is already running.");
            Busy = true; Changed?.Invoke();
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
                var ct = cts.Token;
                using var tcp = new TcpClient();
                using (var connectCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    connectCts.CancelAfter(TimeSpan.FromSeconds(5));
                    try { await tcp.ConnectAsync(host, SyncProtocol.TcpPort, connectCts.Token); }
                    catch (OperationCanceledException) { throw new TimeoutException("PC not found. Is Life OS open on the PC, and are you on the same Wi-Fi?"); }
                }
                using var stream = tcp.GetStream();
                var chan = await SyncProtocol.ClientHandshakeAsync(stream, code, ct);

                JsonObject mine;
                using (var scope = _scopes.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await SyncEngine.EnsureStampsAsync(db);
                    mine = await SyncEngine.ExportAsync(db);
                }
                await chan.SendAsync(SyncPayload.Pack(mine), ct);
                var theirs = SyncPayload.Unpack(await chan.ReceiveAsync(ct));

                SyncStats stats;
                using (var scope = _scopes.CreateScope())
                    stats = await SyncEngine.MergeAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(), theirs);

                LastSyncAt = DateTime.Now;
                LastMessage = $"Synced at {LastSyncAt:HH:mm} ({stats})";
                if (stats.Total > 0) _ui.NotifyDataChanged();
                return (true, LastMessage);
            }
            catch (Exception ex)
            {
                var msg = ex is UnauthorizedAccessException or TimeoutException ? ex.Message : "Sync failed: " + ex.Message;
                if (!silent || LastMessage == null) LastMessage = msg;
                return (false, msg);
            }
            finally { Busy = false; _one.Release(); Changed?.Invoke(); }
        }

        public void Dispose() => _timer?.Dispose();
    }
}
