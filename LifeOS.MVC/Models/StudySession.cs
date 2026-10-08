using LifeOS.MVC.Models;
using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class StudySession
    {
        public int Id { get; set; }

        public int StudySubjectId { get; set; }
        public StudySubject StudySubject { get; set; } = null!;

        public DateTime SessionDate { get; set; } = DateTime.Today;

        public double DurationHours { get; set; }

        [MaxLength(200)]
        public string? TopicCovered { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Range(1, 10)]
        public int FocusRating { get; set; } = 5; // 1–10

        public DateTime LoggedAt { get; set; } = DateTime.Now;
    }
}
