using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class StudySubject
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = "";

        [MaxLength(300)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string Category { get; set; } = "General";
        // e.g. Programming, Math, Language, Religion, Science, Exam Prep

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active | Paused | Completed

        [Range(0, 100000)]
        public int TargetHours { get; set; } = 0;

        public double HoursLogged { get; set; } = 0;

        // Hours planned per week; 0 = no weekly target (used by the dashboard Studies level)
        [Range(0, 168)]
        public double WeeklyTargetHours { get; set; } = 0;

        [Range(0, 100)]
        public int ProgressPercent { get; set; } = 0;

        [MaxLength(200)]
        public string? ResourceUrl { get; set; }

        [MaxLength(200)]
        public string? CurrentTopic { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<StudyTask> Tasks { get; set; } = new List<StudyTask>();
        // Navigation
        public ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
        public ICollection<StudyNote> StudyNotes { get; set; } = new List<StudyNote>();
    }
}
