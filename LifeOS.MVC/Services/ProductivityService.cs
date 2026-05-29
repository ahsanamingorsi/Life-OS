
using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Services
{
    public class ProductivityService
    {
        private readonly AppDbContext _db;

        public ProductivityService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Calculates daily productivity score and assembles the
        /// full ProductivityScoreViewModel for dashboard / module views.
        /// </summary>
        public async Task<ProductivityScoreViewModel> GetDailyScoreAsync(DateTime? date = null)
        {
            var targetDate = (date ?? DateTime.Today).Date;

            var activeHabits = await _db.Habits
                .Where(h => h.IsActive)
                .Include(h => h.HabitLogs)
                .Include(h => h.Streak)
                .ToListAsync();

            var completedHabitIds = await _db.HabitLogs
                .Where(l => l.CompletedDate.Date == targetDate)
                .Select(l => l.HabitId)
                .ToListAsync();

            var totalTasks = await _db.PersonalTasks.CountAsync();
            var completedTasks = await _db.PersonalTasks.CountAsync(t => t.IsCompleted);

            int totalHabits = activeHabits.Count;
            int completedHabits = completedHabitIds.Count;
            int denominator = totalHabits + totalTasks;

            double score = denominator > 0
                ? Math.Round((double)(completedHabits + completedTasks) / denominator * 100, 1)
                : 0;

            var habitsWithStreaks = activeHabits.Select(h => new HabitWithStreakViewModel
            {
                Habit = h,
                CompletedToday = completedHabitIds.Contains(h.Id),
                CurrentStreak = h.Streak?.CurrentStreak ?? 0,
                BestStreak = h.Streak?.BestStreak ?? 0
            }).ToList();

            var recentActivity = await _db.ActivityLogs
                .OrderByDescending(a => a.DateTime)
                .Take(10)
                .ToListAsync();

            return new ProductivityScoreViewModel
            {
                TotalHabits = totalHabits,
                CompletedHabitsToday = completedHabits,
                TotalPersonalTasks = totalTasks,
                CompletedPersonalTasks = completedTasks,
                ProductivityScore = score,
                RecentActivity = recentActivity,
                HabitsWithStreaks = habitsWithStreaks
            };
        }

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