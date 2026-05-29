using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class TechSkill
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(50)]
        public string Category { get; set; } = ""; // e.g. Backend, Frontend, DevOps

        public int ProgressPercent { get; set; } // 0–100

        public double HoursLogged { get; set; }

        public bool IsCurrent { get; set; }

        public bool IsNext { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}