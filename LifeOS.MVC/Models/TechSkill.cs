using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class TechSkill
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(50)]
        public string Category { get; set; } = "Backend";
        // Backend | Frontend | DevOps | Database | Mobile | Tools | Other

        [MaxLength(20)]
        public string Level { get; set; } = "Beginner";
        // Beginner | Intermediate | Advanced | Expert

        [Range(0, 100)]
        public int ProgressPercent { get; set; } = 0; // 0–100

        public double HoursLogged { get; set; } = 0;

        [Range(0, 168)]
        public double WeeklyTargetHours { get; set; } = 1;

        public bool IsCurrent { get; set; } = false;

        public bool IsNext { get; set; } = false;

        [MaxLength(200)]
        public string? ResourceUrl { get; set; }

        [MaxLength(200)]
        public string? ResourceName { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(200)]
        public string? CurrentTopic { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<TechSkillLog> SkillLogs { get; set; } = new List<TechSkillLog>();

        // Not persisted — populated by controller for display
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public double WeeklyHours { get; set; } = 0;
    }
}
