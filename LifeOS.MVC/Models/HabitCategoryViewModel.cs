using LifeOS.MVC.Models;

namespace LifeOS.MVC.Models
{
    public class HabitCategoryViewModel
    {
        /// <summary>The category this page is filtered to (e.g. "Health", "Mind")</summary>
        public string Category { get; set; } = "";

        /// <summary>Display title shown in the page header</summary>
        public string PageTitle { get; set; } = "";

        /// <summary>Subtitle shown under the title</summary>
        public string PageSubtitle { get; set; } = "";

        /// <summary>All habits for this category with streak/completion state</summary>
        public List<HabitWithStreakViewModel> Habits { get; set; } = new();

        /// <summary>All habits in the system (for the create form category default)</summary>
        public List<string> AllCategories { get; set; } = new()
        {
            "Health", "Mind", "Learning", "Spiritual", "Productivity", "General"
        };

        // ── Summary stats ────────────────────────────────────────────
        public int TotalHabits => Habits.Count;
        public int DoneToday => Habits.Count(h => h.CompletedToday);
        public int BestStreak => Habits.Any() ? Habits.Max(h => h.BestStreak) : 0;
        public int TopCurrentStreak => Habits.Any() ? Habits.Max(h => h.CurrentStreak) : 0;
        public int CompletionPercent => TotalHabits > 0
            ? (int)((double)DoneToday / TotalHabits * 100) : 0;

        // ── Last 7 days completion data (for the mini heatmap) ────────
        /// <summary>Key = date, Value = count of habits completed that day</summary>
        public Dictionary<DateTime, int> Last7DaysCompletion { get; set; } = new();
    }
}