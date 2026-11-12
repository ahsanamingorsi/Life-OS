using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Services
{
    public class ProductivityService
    {
        private readonly AppDbContext _db;
        private readonly TaskService _tasks;

        public ProductivityService(AppDbContext db, TaskService tasks)
        {
            _db = db;
            _tasks = tasks;
        }

        /// <summary>
        /// Daily productivity score = equal average of the areas that had something
        /// to measure that day: Tasks, Projects, Studies, Skills and Spiritual.
        /// Goals and Review are not part of the daily score.
        /// </summary>
        public async Task<ProductivityScoreViewModel> GetDailyScoreAsync(DateTime? date = null)
        {
            var day = (date ?? DateTime.Today).Date;
            var next = day.AddDays(1);
            var areas = new List<AreaScore>();

            // ── Tasks: one-time tasks due that day + repeated tasks that occur that day ──
            var dayTasks = await _tasks.GetForDayAsync(day, includeOverdue: false);
            int totalTasks = dayTasks.Count;
            int doneTasks = dayTasks.Count(t => t.IsDone);
            if (totalTasks > 0)
                areas.Add(new AreaScore { Name = "Tasks", Percent = Pct(doneTasks, totalTasks), Value = $"{doneTasks}/{totalTasks}" });

            // ── Projects: project tasks finished that day vs those that were due by then ──
            int projectDone = await _db.ProjectTasks.CountAsync(t => t.CompletedAt >= day && t.CompletedAt < next);
            int projectOpenDue = await _db.ProjectTasks.CountAsync(t =>
                t.DueDate < next && (!t.IsCompleted || t.CompletedAt >= next));
            if (projectDone + projectOpenDue > 0)
                areas.Add(new AreaScore { Name = "Projects", Percent = Pct(projectDone, projectDone + projectOpenDue), Value = $"{projectDone} done" });

            // ── Studies: hours that day vs one seventh of the weekly target ──
            double studyDailyTarget = await _db.StudySubjects
                .Where(s => s.Status == "Active")
                .SumAsync(s => s.WeeklyTargetHours) / 7.0;
            double studyHours = await _db.StudySessions
                .Where(s => s.SessionDate >= day && s.SessionDate < next)
                .SumAsync(s => s.DurationHours);
            if (studyDailyTarget > 0)
                areas.Add(new AreaScore { Name = "Studies", Percent = Pct(studyHours, studyDailyTarget), Value = $"{studyHours:0.#}h" });

            // ── Skills: hours that day vs one seventh of the weekly target ──
            // Target = current skill plus any skill worked on in that week.
            var weekStart = WeekStart(day);
            double skillDailyTarget = await _db.TechSkills
                .Where(s => s.IsCurrent || s.SkillLogs.Any(l => l.LogDate >= weekStart && l.LogDate < weekStart.AddDays(7)))
                .SumAsync(s => s.WeeklyTargetHours) / 7.0;
            double skillHours = await _db.TechSkillLogs
                .Where(l => l.LogDate >= day && l.LogDate < next)
                .SumAsync(l => l.Hours);
            if (skillDailyTarget > 0)
                areas.Add(new AreaScore { Name = "Skills", Percent = Pct(skillHours, skillDailyTarget), Value = $"{skillHours:0.#}h" });

            // ── Spiritual: prayers that day (always counted; default 5) ──
            var spiritual = await _db.SpiritualEntries.AsNoTracking()
                .Where(s => s.Date >= day && s.Date < next)
                .OrderBy(s => s.Id)
                .FirstOrDefaultAsync();
            int prayerTarget = spiritual is { PrayersTotal: > 0 } ? spiritual.PrayersTotal : 5;
            int prayersDone = Math.Clamp(spiritual?.PrayersDone ?? 0, 0, prayerTarget);
            areas.Add(new AreaScore { Name = "Spiritual", Percent = Pct(prayersDone, prayerTarget), Value = $"{prayersDone}/{prayerTarget}" });

            double score = areas.Count > 0 ? Math.Round(areas.Average(a => a.Percent), 1) : 0;

            var recentActivity = await _db.ActivityLogs.AsNoTracking()
                .OrderByDescending(a => a.DateTime)
                .Take(10)
                .ToListAsync();

            return new ProductivityScoreViewModel
            {
                TotalPersonalTasks = totalTasks,
                CompletedPersonalTasks = doneTasks,
                TotalPrayersToday = prayerTarget,
                PrayersCompletedToday = prayersDone,
                ProductivityScore = score,
                HasData = areas.Count > 0,
                Areas = areas,
                RecentActivity = recentActivity,
            };
        }

        /// <summary>Monday of the week that contains <paramref name="day"/>.</summary>
        public static DateTime WeekStart(DateTime day)
        {
            int offset = ((int)day.DayOfWeek + 6) % 7; // Monday = 0
            return day.Date.AddDays(-offset);
        }

        public static int Pct(double done, double total) =>
            total <= 0 ? 0 : (int)Math.Round(Math.Clamp(done / total, 0, 1) * 100);

        /// <summary>
        /// Writes one ActivityLog row. Call from controllers after
        /// any meaningful state change.
        /// </summary>
        public async Task LogActivityAsync(string type, int referenceId, string action, string? detail = null)
        {
            _db.ActivityLogs.Add(new ActivityLog
            {
                Type = type,
                ReferenceId = referenceId,
                Action = action,
                Detail = detail,
                DateTime = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }
    }
}
