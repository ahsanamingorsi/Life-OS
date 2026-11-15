using LifeOS.MVC.Models;

namespace LifeOS.MVC.Models
{
    public class StudyDashboardViewModel
    {
        public List<StudySubject> Subjects { get; set; } = new();

        // Stats
        public double TotalHoursAllTime => Subjects.Sum(s => s.StudySessions.Sum(x => x.DurationHours));
        public int ActiveSubjects => Subjects.Count(s => s.Status == "Active");
        public int CompletedSubjects => Subjects.Count(s => s.Status == "Completed");
        public int TotalNotes => Subjects.Sum(s => s.StudyNotes.Count);

        // This week sessions
        public List<StudySession> ThisWeekSessions { get; set; } = new();
        public double ThisWeekHours => ThisWeekSessions.Sum(s => s.DurationHours);

        // Today's sessions
        public List<StudySession> TodaySessions { get; set; } = new();
        public double TodayHours => TodaySessions.Sum(s => s.DurationHours);

        // Recent notes
        public List<StudyNote> RecentNotes { get; set; } = new();
    }
}