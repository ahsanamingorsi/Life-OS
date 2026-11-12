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

        // Encrypted with Windows DPAPI by VaultCrypto ("dpapi:" + base64). Never sent to the page;
        // the Show / Copy buttons fetch it from VaultController.Reveal.
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

    }
}