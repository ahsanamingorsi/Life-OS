using LifeOS.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.App.Services
{
    public static class StudyProgress
    {
        /// <summary>Total hours come from the sessions; progress averages hours-vs-target and tasks done.</summary>
        public static async Task RecalculateAsync(AppDbContext db, int subjectId)
        {
            var subject = await db.StudySubjects.FindAsync(subjectId);
            if (subject == null) return;
            var done = await db.StudyTasks.Where(t => t.StudySubjectId == subjectId).Select(t => t.IsCompleted).ToListAsync();
            subject.HoursLogged = await db.StudySessions.Where(x => x.StudySubjectId == subjectId).SumAsync(x => x.DurationHours);

            var parts = new List<double>();
            if (subject.TargetHours > 0) parts.Add(Math.Min(1.0, subject.HoursLogged / subject.TargetHours));
            if (done.Any()) parts.Add((double)done.Count(d => d) / done.Count);
            if (parts.Any()) subject.ProgressPercent = Math.Clamp((int)Math.Round(parts.Average() * 100), 0, 100);
            await db.SaveChangesAsync();
        }
    }
}
