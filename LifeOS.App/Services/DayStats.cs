using LifeOS.MVC.Data;
using LifeOS.MVC.Services;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.App.Services
{
    /// <summary>Numbers for one day or one week, used by the evening shutdown and the weekly review.</summary>
    public record PeriodStats(
        DateTime From, DateTime To,
        int TasksDone, int TasksTotal,
        int MitsDone, int MitsTotal,
        int FocusMinutes,
        int HabitsDone, int HabitsTotal,
        double StudyHours, double SkillHours,
        double AvgScore, int ScoredDays,
        double AvgMood, int MoodDays,
        int PrayersDone, int PrayersTotal);

    public static class DayStats
    {
        public static async Task<PeriodStats> LoadAsync(IServiceProvider sp, DateTime from, DateTime to)
        {
            var db = sp.GetRequiredService<AppDbContext>();
            var prod = sp.GetRequiredService<ProductivityService>();
            from = from.Date; to = to.Date;
            var end = to.AddDays(1);

            int tasksDone = 0, tasksTotal = 0, prayersDone = 0, prayersTotal = 0, scoredDays = 0;
            double scoreSum = 0;
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                var s = await prod.GetDailyScoreAsync(d);
                tasksDone += s.CompletedPersonalTasks; tasksTotal += s.TotalPersonalTasks;
                prayersDone += s.PrayersCompletedToday; prayersTotal += s.TotalPrayersToday;
                if (s.HasData) { scoreSum += s.ProductivityScore; scoredDays++; }
            }

            var plan = await db.PlanItems.AsNoTracking().Where(p => p.Date >= from && p.Date <= to && p.IsMit).ToListAsync();
            int focus = await db.FocusSessions.Where(f => f.StartedAt >= from && f.StartedAt < end).SumAsync(f => (int?)f.Minutes) ?? 0;

            var habits = await db.Habits.AsNoTracking().Where(h => !h.IsArchived).ToListAsync();
            var logs = await db.HabitLogs.AsNoTracking().Where(l => l.Date >= from && l.Date <= to).ToListAsync();
            int hDone = 0, hTotal = 0;
            for (var d = from; d <= to; d = d.AddDays(1))
                foreach (var h in habits.Where(h => h.CreatedAt.Date <= d && HabitMath.IsActiveOn(h, d)))
                {
                    hTotal++;
                    var v = logs.FirstOrDefault(l => l.HabitId == h.Id && l.Date == d)?.Value ?? 0;
                    if (HabitMath.IsDone(h, v)) hDone++;
                }

            double study = await db.StudySessions.Where(s => s.SessionDate >= from && s.SessionDate < end).SumAsync(s => (double?)s.DurationHours) ?? 0;
            double skill = await db.TechSkillLogs.Where(l => l.LogDate >= from && l.LogDate < end).SumAsync(l => (double?)l.Hours) ?? 0;

            var moods = await db.DayReflections.AsNoTracking().Where(r => r.Date >= from && r.Date <= to && r.Mood > 0).Select(r => r.Mood).ToListAsync();

            return new PeriodStats(from, to, tasksDone, tasksTotal, plan.Count(p => p.IsDone), plan.Count, focus, hDone, hTotal,
                Math.Round(study, 1), Math.Round(skill, 1),
                scoredDays > 0 ? Math.Round(scoreSum / scoredDays) : 0, scoredDays,
                moods.Count > 0 ? Math.Round(moods.Average(), 1) : 0, moods.Count,
                prayersDone, prayersTotal);
        }
    }
}
