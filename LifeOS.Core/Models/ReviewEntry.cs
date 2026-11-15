using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class ReviewEntry
    {
        public int Id { get; set; }

        [MaxLength(10)]
        public string Period { get; set; } = "Weekly"; // Weekly or Monthly

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [MaxLength(1000)]
        public string? Wins { get; set; }

        [MaxLength(1000)]
        public string? MissedGoals { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(1000)]
        public string? ImprovementSuggestions { get; set; }

        public int OverallScore { get; set; } // 1–10

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}