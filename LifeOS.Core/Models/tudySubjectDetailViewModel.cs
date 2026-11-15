using LifeOS.MVC.Models;

namespace LifeOS.MVC.Models
{
    public class StudySubjectDetailViewModel
    {
        public StudySubject Subject { get; set; } = null!;

        public List<StudySession> Sessions { get; set; } = new();
        public List<StudyNote> Notes { get; set; } = new();

        // Stats
        public double TotalHours => Sessions.Sum(s => s.DurationHours);
        // This week = since Monday (same as the dashboard)
        public double WeekHours => Sessions
            .Where(s => s.SessionDate >= LifeOS.MVC.Services.ProductivityService.WeekStart(DateTime.Today))
            .Sum(s => s.DurationHours);
        public double AvgFocusRating => Sessions.Any()
            ? Math.Round(Sessions.Average(s => s.FocusRating), 1) : 0;
        public int TotalSessions => Sessions.Count;

        // Last 7 days heatmap: date → hours
        public Dictionary<DateTime, double> Last7Days { get; set; } = new();
    }
}