using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = "";

        [MaxLength(50)]
        public string Category { get; set; } = ""; // EduBridgePath, PSC, Farming, etc.

        [MaxLength(30)]
        public string Status { get; set; } = "Active"; // Active, Paused, Done

        [MaxLength(1)]
        public string Priority { get; set; } = "B"; // A, B, C

        public int ProgressPercent { get; set; }

        [MaxLength(300)]
        public string? NextTask { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime? Deadline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}