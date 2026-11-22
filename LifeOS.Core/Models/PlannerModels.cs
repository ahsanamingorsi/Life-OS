using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    /// <summary>One item in a day's plan: a "most important thing" and/or a time block.</summary>
    public class PlanItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        /// <summary>One of the day's 3 most important things.</summary>
        public bool IsMit { get; set; }

        /// <summary>Minutes since midnight (null = not scheduled at a time).</summary>
        public int? StartMinutes { get; set; }
        public int? EndMinutes { get; set; }

        /// <summary>Free | Task | ProjectTask | StudyTask | Skill</summary>
        [MaxLength(20)]
        public string SourceKind { get; set; } = "Free";
        public int? SourceId { get; set; }

        public bool IsDone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Habit
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(20)]
        public string Icon { get; set; } = "sparkle";

        /// <summary>Check (done / not done) or Count (reach a target number).</summary>
        [MaxLength(10)]
        public string Kind { get; set; } = "Check";
        public int Target { get; set; } = 1;

        [MaxLength(20)]
        public string? Unit { get; set; }

        /// <summary>Bit mask of active weekdays, same as tasks: Monday = 1 ... Sunday = 64 (127 = every day).</summary>
        public int DaysMask { get; set; } = 127;

        public bool IsArchived { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<HabitLog> Logs { get; set; } = new();
    }

    public class HabitLog
    {
        public int Id { get; set; }
        public int HabitId { get; set; }
        public Habit? Habit { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public int Value { get; set; }
    }

    /// <summary>A finished focus block, logged by the timer.</summary>
    public class FocusSession
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public int Minutes { get; set; }

        /// <summary>Free | Task | ProjectTask | StudyTask | Skill</summary>
        [MaxLength(20)]
        public string Kind { get; set; } = "Free";
        public int? RefId { get; set; }

        [MaxLength(200)]
        public string? Label { get; set; }
    }

    /// <summary>Evening shutdown / reflection for one day.</summary>
    public class DayReflection
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;

        [MaxLength(1000)]
        public string? Wins { get; set; }
        [MaxLength(1000)]
        public string? Slipped { get; set; }
        [MaxLength(1000)]
        public string? Gratitude { get; set; }

        /// <summary>1 (bad) to 5 (great); 0 = not set.</summary>
        public int Mood { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>Simple key/value app settings (prayer times, reminder switches, ...).</summary>
    public class AppSetting
    {
        [Key, MaxLength(100)]
        public string Key { get; set; } = "";

        [MaxLength(1000)]
        public string Value { get; set; } = "";
    }
}
