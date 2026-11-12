namespace LifeOS.MVC.Models
{
    public class ProductivityScoreViewModel
    {
        // Tasks due that day (one-time tasks due + repeated tasks that occur that day)
        public int TotalPersonalTasks { get; set; }
        public int CompletedPersonalTasks { get; set; }

        public double ProductivityScore { get; set; } // 0–100 (0 when HasData is false)
        public bool HasData { get; set; }               // false = no area had anything to measure that day
        public List<AreaScore> Areas { get; set; } = new(); // only the areas counted in the score
        public List<ActivityLog> RecentActivity { get; set; } = new();

        public int TotalPrayersToday { get; set; }
        public int PrayersCompletedToday { get; set; }
    }

    /// <summary>One area's share of a day's productivity score.</summary>
    public class AreaScore
    {
        public string Name { get; set; } = "";
        public int Percent { get; set; }          // 0–100
        public string Value { get; set; } = "";   // short text, e.g. "6/9" or "1.5h"
    }
}
