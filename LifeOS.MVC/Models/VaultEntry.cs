using System.ComponentModel.DataAnnotations;

namespace LifeOS.Models
{
    public class VaultEntry
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = "";
        // e.g. "Gmail", "GitHub", "Bank Account"

        [MaxLength(50)]
        public string Category { get; set; } = "Account";
        // Account | Social | Bank | Email | Dev | Subscription | Other

        [MaxLength(200)]
        public string? Username { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        // Stored as base64 simple encoding
        // NOTE: For production use proper encryption (AES etc.)
        // This is a personal local system so basic obfuscation is used
        [MaxLength(500)]
        public string? PasswordEncoded { get; set; }

        [MaxLength(500)]
        public string? Url { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(300)]
        public string? RecoveryEmail { get; set; }

        [MaxLength(300)]
        public string? PhoneNumber { get; set; }

        public bool IsFavorite { get; set; } = false;

        public bool TwoFactorEnabled { get; set; } = false;

        [MaxLength(300)]
        public string? TwoFactorMethod { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Helper — NOT stored
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? PasswordPlain
        {
            get
            {
                if (string.IsNullOrEmpty(PasswordEncoded)) return null;
                try
                {
                    return System.Text.Encoding.UTF8.GetString(
                        Convert.FromBase64String(PasswordEncoded));
                }
                catch { return PasswordEncoded; }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    PasswordEncoded = null;
                else
                    PasswordEncoded = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes(value));
            }
        }
    }
}