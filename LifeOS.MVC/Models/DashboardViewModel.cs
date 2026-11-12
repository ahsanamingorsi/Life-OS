namespace LifeOS.MVC.Models
{
    public class DashboardViewModel
    {
        public DateTime Today { get; set; } = DateTime.Today;
        public string Greeting { get; set; } = "Good morning";

        // ── Today's productivity (ring) ───────────────────────────────
        public bool HasScore { get; set; }
        public int Score { get; set; }
        public int? ScoreChange { get; set; }            // vs yesterday, null when either day has no data
        public string ScoreLabel { get; set; } = "";
        public List<AreaScore> ScoreParts { get; set; } = new();

        // ── Today's tasks ─────────────────────────────────────────────
        public List<DashboardTask> Tasks { get; set; } = new();
        public int TasksDueToday { get; set; }
        public int TasksDoneToday { get; set; }

        // ── Spiritual today ───────────────────────────────────────────
        public int PrayersDone { get; set; }
        public int PrayersTotal { get; set; } = 5;
        public int PrayerStreak { get; set; }

        // ── Needs attention ───────────────────────────────────────────
        public List<DashboardAlert> Alerts { get; set; } = new();

        // ── Productivity by area (this week) ──────────────────────────
        public List<AreaLevel> Levels { get; set; } = new();

        // ── Projects in focus ─────────────────────────────────────────
        public List<Project> FocusProjects { get; set; } = new();

        // ── Last 7 days ───────────────────────────────────────────────
        public List<DayScore> Week { get; set; } = new();

        // ── Library ───────────────────────────────────────────────────
        public int LinkCount { get; set; }
        public int LinksPinned { get; set; }
        public int VaultCount { get; set; }
        public int VaultWithout2FA { get; set; }

        // ── Recent activity ───────────────────────────────────────────
        public List<ActivityLog> RecentActivity { get; set; } = new();
    }

    public class DashboardTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Meta { get; set; } = "";
        public bool IsCompleted { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsRepeated { get; set; }
        public string Repeat { get; set; } = "None";
        public string Priority { get; set; } = "B";
    }

    public class DashboardAlert
    {
        public string Text { get; set; } = "";
        public string Url { get; set; } = "/";
    }

    public class AreaLevel
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Url { get; set; } = "/";
        public int? Percent { get; set; }          // null = nothing to measure yet
        public string Detail { get; set; } = "";
    }

    public class DayScore
    {
        public DateTime Date { get; set; }
        public int? Score { get; set; }             // null = no data that day
    }
}
