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
        public double WeekHours => Sessions
            .Where(s => s.SessionDate >= DateTime.Today.AddDays(-7))
            .Sum(s => s.DurationHours);
        public double AvgFocusRating => Sessions.Any()
            ? Math.Round(Sessions.Average(s => s.FocusRating), 1) : 0;
        public int TotalSessions => Sessions.Count;

        // Last 7 days heatmap: date → hours
        public Dictionary<DateTime, double> Last7Days { get; set; } = new();
    }
}