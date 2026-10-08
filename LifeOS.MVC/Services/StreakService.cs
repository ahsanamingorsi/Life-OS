
using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Services
{
    public class StreakService
    {
        private readonly AppDbContext _db;

        public StreakService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Recalculates the current and best streak for a habit
        /// based on its full HabitLog history. Call this after
        /// every completion or un-completion.
        /// </summary>
        public async Task RecalculateAsync(int habitId)
        {
            var habit = await _db.Habits.FindAsync(habitId);
            if (habit == null) return;

            // Pull all completed dates sorted descending
            var dates = await _db.HabitLogs
                .Where(l => l.HabitId == habitId)
                .OrderByDescending(l => l.CompletedDate)
                .Select(l => l.CompletedDate.Date)
                .ToListAsync();

            if (!dates.Any())
            {
                await UpsertStreakAsync(habitId, 0, 0, null);
                return;
            }

            // ── Current streak ───────────────────────────────────────────
            int current = 0;
            var today = DateTime.Today;
            var cursor = dates[0]; // most recent completed date

            // Allow today OR yesterday as starting point (so you don't
            // lose streak if you haven't marked today yet)
            if (cursor == today || cursor >= today.AddDays(habit.Frequency == "Weekly" ? -7 : -1) && cursor <= today)
            {
                current = 1;
                for (int i = 1; i < dates.Count; i++)
                {
                    var expected = cursor.AddDays(habit.Frequency == "Weekly" ? -7 : -1);
                    if (dates[i] == expected)
                    {
                        current++;
                        cursor = dates[i];
                    }
                    else break;
                }
            }

            // ── Best streak ──────────────────────────────────────────────
            int best = 1, runLen = 1;
            for (int i = 1; i < dates.Count; i++)
            {
                int gap = habit.Frequency == "Weekly" ? 7 : 1;
                if ((dates[i - 1] - dates[i]).Days == gap)
                    runLen++;
                else
                    runLen = 1;

                if (runLen > best) best = runLen;
            }
            if (runLen > best) best = runLen;
            if (current > best) best = current;

            await UpsertStreakAsync(habitId, current, best, dates[0]);
        }

        private async Task UpsertStreakAsync(int habitId, int current, int best, DateTime? lastDate)
        {
            var streak = await _db.Streaks.FirstOrDefaultAsync(s => s.HabitId == habitId);
            if (streak == null)
            {
                _db.Streaks.Add(new Streak
                {
                    HabitId = habitId,
                    CurrentStreak = current,
                    BestStreak = best,
                    LastCompletedDate = lastDate,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                streak.CurrentStreak = current;
                if (best > streak.BestStreak) streak.BestStreak = best;
                streak.LastCompletedDate = lastDate;
                streak.UpdatedAt = DateTime.Now;
            }
            await _db.SaveChangesAsync();
        }
    }
}
