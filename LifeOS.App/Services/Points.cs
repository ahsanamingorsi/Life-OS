using LifeOS.MVC.Data;
using LifeOS.MVC.Services;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.App.Services
{
    public record PointsResult(int Points, int Level, int LevelStart, int NextLevelAt);

    /// <summary>
    /// Light "game" layer, worked out from your real data (nothing extra is stored):
    /// task done 10, top-3 item done 20, habit done 5, 5 focus minutes 1, prayer 3, study/skill hour 10.
    /// </summary>
    public static class Points
    {
        public static async Task<PointsResult> ComputeAsync(AppDbContext db)
        {
            long total = 0;
            total += 10L * await db.PersonalTasks.CountAsync(t => t.IsCompleted && t.Repeat == "None");
            total += 10L * await db.TaskCompletions.CountAsync();
            total += 10L * await db.ProjectTasks.CountAsync(t => t.IsCompleted);
            total += 10L * await db.StudyTasks.CountAsync(t => t.IsCompleted);
            total += 20L * await db.PlanItems.CountAsync(p => p.IsMit && p.IsDone);
            total += (await db.FocusSessions.SumAsync(f => (int?)f.Minutes) ?? 0) / 5;
            total += 3L * (await db.SpiritualEntries.SumAsync(s => (int?)s.PrayersDone) ?? 0);
            total += (long)(10 * ((await db.StudySessions.SumAsync(s => (double?)s.DurationHours) ?? 0) + (await db.TechSkillLogs.SumAsync(l => (double?)l.Hours) ?? 0)));

            var habits = await db.Habits.AsNoTracking().ToDictionaryAsync(h => h.Id);
            foreach (var l in await db.HabitLogs.AsNoTracking().ToListAsync())
                if (habits.TryGetValue(l.HabitId, out var h) && HabitMath.IsDone(h, l.Value)) total += 5;

            int points = (int)Math.Min(total, int.MaxValue);
            int level = 1 + (int)Math.Floor(Math.Sqrt(points / 40.0));
            return new PointsResult(points, level, 40 * (level - 1) * (level - 1), 40 * level * level);
        }
    }
}
