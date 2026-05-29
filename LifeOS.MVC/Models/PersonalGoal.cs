using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class PersonalGoal
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Goal { get; set; } = "";

        [MaxLength(50)]
        public string Category { get; set; } = ""; // Grooming, Confidence, Lifestyle, Standard

        public int ProgressPercent { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        public bool IsAchieved { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}