using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class PersonalTask
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        // One-time task: the day it is due.
        // Repeated task: the first day it repeats from (start date).
        public DateTime? DueDate { get; set; }

        [MaxLength(10), RegularExpression("A|B|C")]
        public string Priority { get; set; } = "B"; // A | B | C

        // ── Repeat ───────────────────────────────────────────────────
        [MaxLength(10), RegularExpression("None|Daily|Weekly|Monthly")]
        public string Repeat { get; set; } = "None";

        // Weekly: days as bit flags, Monday = 1, Tuesday = 2, Wednesday = 4,
        // Thursday = 8, Friday = 16, Saturday = 32, Sunday = 64.
        [Range(0, 127)]
        public int RepeatDays { get; set; }

        // Monthly: day of month (1–31). Shorter months use their last day.
        [Range(1, 31)]
        public int? RepeatDayOfMonth { get; set; }

        // Optional last day a repeated task appears.
        public DateTime? RepeatEndDate { get; set; }

        // ── Goal link (optional) ─────────────────────────────────────
        public int? GoalId { get; set; }
        public PersonalGoal? Goal { get; set; }

        // One-time tasks only. Repeated tasks use TaskCompletions per day.
        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<TaskCompletion> Completions { get; set; } = new List<TaskCompletion>();

        public bool IsRepeated => Repeat != "None";
    }

    /// <summary>One day on which a repeated task was completed.</summary>
    public class TaskCompletion
    {
        public int Id { get; set; }
        public int PersonalTaskId { get; set; }
        public PersonalTask? PersonalTask { get; set; }
        public DateTime Date { get; set; }          // the day the occurrence belongs to
        public DateTime CompletedAt { get; set; } = DateTime.Now;
    }
}
