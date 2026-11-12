namespace LifeOS.MVC.Models
{
    public class TasksPageViewModel
    {
        public DateTime Today { get; set; } = DateTime.Today;
        public string Filter { get; set; } = "open";   // open | today | repeated | overdue | done | all
        public int? GoalId { get; set; }
        public List<TaskRow> Rows { get; set; } = new();
        public List<PersonalGoal> Goals { get; set; } = new();

        public int OpenCount { get; set; }
        public int TodayCount { get; set; }
        public int TodayDone { get; set; }
        public int RepeatedCount { get; set; }
        public int OverdueCount { get; set; }
        public int DoneCount { get; set; }
    }

    public class TaskRow
    {
        public PersonalTask Task { get; set; } = null!;
        public string RepeatText { get; set; } = "";
        public bool OccursToday { get; set; }     // repeated: appears today
        public bool DoneToday { get; set; }       // repeated: completed today
        public bool IsOverdue { get; set; }       // one-time: open and past due
        public int Streak { get; set; }
        public int TimesDone { get; set; }        // repeated: total completions
        public DateTime? NextDate { get; set; }   // repeated: next day it appears (today or later)
    }

    public class TaskFormViewModel
    {
        public PersonalTask Task { get; set; } = new();
        public List<PersonalGoal> Goals { get; set; } = new();
        public string? ReturnUrl { get; set; }
    }
}
