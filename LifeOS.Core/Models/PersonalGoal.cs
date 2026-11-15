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

        // Manual progress. Used only when no task is linked to this goal;
        // otherwise progress is calculated from the linked tasks.
        [Range(0, 100)]
        public int ProgressPercent { get; set; }

        // How many times the goal's repeated tasks should be completed in total
        // (e.g. 60 workouts). Null = repeated tasks are not counted toward progress.
        [Range(1, 100000)]
        public int? TargetCount { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        public bool IsAchieved { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<PersonalTask> Tasks { get; set; } = new List<PersonalTask>();
    }
}
