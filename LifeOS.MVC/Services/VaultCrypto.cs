using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace LifeOS.MVC.Services
{
    /// <summary>
    /// Encrypts vault passwords with Windows DPAPI (current Windows user only).
    /// Stored format: "dpapi:" + base64 of the protected bytes.
    /// Passwords saved by this app can only be read back by the same Windows user on this computer.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class VaultCrypto
    {
        private const string Prefix = "dpapi:";
        private const int MaxPasswordLength = 100; // keeps the stored value inside the 500-character column
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("LifeOS.Vault.v1");

        public static bool IsTooLong(string? plain) => plain?.Length > MaxPasswordLength;

        public static string? Encrypt(string? plain)
        {
            if (string.IsNullOrEmpty(plain)) return null;
            var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(plain), Entropy, DataProtectionScope.CurrentUser);
            return Prefix + Convert.ToBase64String(bytes);
        }

        /// <summary>Returns the password, or null when it cannot be read (e.g. another Windows user).</summary>
        public static string? Decrypt(string? stored)
        {
            if (string.IsNullOrEmpty(stored)) return null;
            try
            {
                if (stored.StartsWith(Prefix, StringComparison.Ordinal))
                {
                    var bytes = ProtectedData.Unprotect(Convert.FromBase64String(stored[Prefix.Length..]), Entropy, DataProtectionScope.CurrentUser);
                    return Encoding.UTF8.GetString(bytes);
                }
                // Older entries were only base64-encoded (none exist today; kept so they still open).
                return Encoding.UTF8.GetString(Convert.FromBase64String(stored));
            }
            catch (Exception ex) when (ex is CryptographicException or FormatException)
            {
                return null;
            }
        }
    }
}
