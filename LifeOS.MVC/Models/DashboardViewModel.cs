using LifeOS.MVC.Models;

namespace LifeOS.MVC.Models
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = "Ahsan Amin Gorsi";
        public DateTime Today { get; set; } = DateTime.Today;
        public string DailyMotivation { get; set; } = "";
        public string TodaysMission { get; set; } = "";

        // ── Quick Stats ──────────────────────────────────────────────
        public int ActiveProjects { get; set; }
        public int SkillsInProgress { get; set; }

        // ── Top Priorities ───────────────────────────────────────────
        public List<Project> TopPriorities { get; set; } = new();

        // ── Current Skill ────────────────────────────────────────────
        public TechSkill? CurrentSkill { get; set; }

        // ── Spiritual (kept separate — prayer is not a generic habit) ─
        public SpiritualEntry? TodaySpiritual { get; set; }

        // ── Module Progress (0–100) ───────────────────────────────────
        public int TechProgress { get; set; }
        public int ProjectsProgress { get; set; }
        public int HealthProgress { get; set; }      // % of Health habits done today
        public int MindProgress { get; set; }        // % of Mind habits done today
        public int SpiritualProgress { get; set; }
        public int PersonalProgress { get; set; }

        // ── Unified Productivity (from ProductivityService) ───────────
        public double ProductivityScore { get; set; }
        public int TodayHabitsTotal { get; set; }
        public int TodayHabitsDone { get; set; }
        public int TotalPersonalTasks { get; set; }
        public int CompletedPersonalTasks { get; set; }
        public int BestHabitStreak { get; set; }
        public int CurrentTopStreak { get; set; }
        public List<ActivityLog> RecentActivityLogs { get; set; } = new();
    }
}