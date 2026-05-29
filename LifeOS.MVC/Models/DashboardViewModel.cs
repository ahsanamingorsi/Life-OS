using LifeOS.MVC.Models;

namespace LifeOS.MVC.Models
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = "Commander";
        public DateTime Today { get; set; } = DateTime.Today;
        public string DailyMotivation { get; set; } = "";
        public string TodaysMission { get; set; } = "";

        // Quick Stats
        public int ActiveProjects { get; set; }
        public int SkillsInProgress { get; set; }
        public int TodayHabitsCompleted { get; set; }
        public int TotalHabits { get; set; }

        // Top Priorities (projects with priority A)
        public List<Project> TopPriorities { get; set; } = new();

        // Current skill
        public TechSkill? CurrentSkill { get; set; }

        // Today's health
        public HealthEntry? TodayHealth { get; set; }

        // Today's spiritual
        public SpiritualEntry? TodaySpiritual { get; set; }

        // Progress Overview (all modules %)
        public int TechProgress { get; set; }
        public int ProjectsProgress { get; set; }
        public int HealthProgress { get; set; }
        public int MindProgress { get; set; }
        public int SpiritualProgress { get; set; }
        public int PersonalProgress { get; set; }

        // ── PRODUCTIVITY MODULE (added) ──────────────────────────────────
        public double ProductivityScore { get; set; }
        public int TodayHabitsTotal { get; set; }
        public int TodayHabitsDone { get; set; }
        public int TotalPersonalTasks { get; set; }
        public int CompletedPersonalTasks { get; set; }
        public int BestHabitStreak { get; set; }
        public int CurrentTopStreak { get; set; }
        public List<LifeOS.MVC.Models.ActivityLog> RecentActivityLogs { get; set; } = new();
    }
}