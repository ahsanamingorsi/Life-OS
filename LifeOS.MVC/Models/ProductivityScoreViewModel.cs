using LifeOS.MVC.Models;

namespace LifeOS.MVC.Models
{
    public class ProductivityScoreViewModel
    {
        public int TotalHabits { get; set; }
        public int CompletedHabitsToday { get; set; }
        public int TotalPersonalTasks { get; set; }
        public int CompletedPersonalTasks { get; set; }
        public double ProductivityScore { get; set; } // 0–100
        public List<ActivityLog> RecentActivity { get; set; } = new();
        public List<HabitWithStreakViewModel> HabitsWithStreaks { get; set; } = new();
    }

    public class HabitWithStreakViewModel
    {
        public Habit Habit { get; set; } = null!;
        public bool CompletedToday { get; set; }
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
    }
}