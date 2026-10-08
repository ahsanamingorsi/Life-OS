
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
        // Assume using System.Threading.Tasks; etc. are present

        public async Task<ProductivityScoreViewModel> GetDailyScoreAsync(DateTime? date = null)
        {
            // 1. Determine the target day (Start of day in UTC or local depending on your DB setting, usually Date is sufficient)
            var targetDate = (date ?? DateTime.Today).Date;

            // --- A. Calculate Habits & Tasks (Existing Logic) ---
            var activeHabits = await _db.Habits
                .Where(h => h.IsActive)
                .Include(h => h.HabitLogs)
                .Include(h => h.Streak)
                .ToListAsync();

            // Get Habit IDs completed TODAY
            var completedHabitIds = await _db.HabitLogs
                .Where(l => l.CompletedDate.Date == targetDate)
                .Select(l => l.HabitId)
                .ToListAsync();

            var totalTasks = await _db.PersonalTasks.CountAsync(t => t.DueDate == targetDate);
            // Ensure you are counting only tasks where the completion date matches today too, 
            // or if PersonalTasks don't have a specific 'CompletedDate' column, adjust logic accordingly.
            // Assuming PersonalTasks.IsCompleted means it was done recently, but for daily score:
            var completedTasks = await _db.PersonalTasks.CountAsync(t => t.DueDate == targetDate && t.IsCompleted);

            int totalHabits = activeHabits.Count;
            int completedHabits = activeHabits.Count(h => completedHabitIds.Contains(h.Id));

            // --- B. Calculate Prayers (NEW Logic) ---
            // Query spiritual entries where the Date matches today's date
            var completedPrayerEntries = await _db.SpiritualEntries
                .Where(p => p.Date == targetDate)
                .ToListAsync();

            int totalPrayersToday = 0;
            if (completedPrayerEntries.Any())
            {
                // Sum up the prayers done across all entries for today
                totalPrayersToday = completedPrayerEntries.Sum(p => p.PrayersDone);

                // Optional: Track total capacity if you have multiple entries per day, 
                // but usually PrayersDone is the aggregate metric. 
                // If your design allows multiple rows per day with different totals, this logic works.
            }

            // --- C. Calculate Denominator and Score ---
            int completedPrayers = totalPrayersToday;

            // Denominator includes Habits + Tasks + Prayers (as requested to add prayers)
            int prayerTarget = completedPrayerEntries.FirstOrDefault()?.PrayersTotal ?? 5;
            completedPrayers = Math.Clamp(completedPrayers, 0, prayerTarget);
            int denominator = totalHabits + totalTasks + prayerTarget; // +1 to prevent division by zero if all are 0

            double score = denominator > 0
                ? Math.Round((double)(completedHabits + completedTasks + completedPrayers) / denominator * 100, 1)
                : 0;

            // --- D. Build Habit Details ---
            var habitsWithStreaks = activeHabits.Select(h => new HabitWithStreakViewModel
            {
                Habit = h,
                CompletedToday = completedHabitIds.Contains(h.Id),
                CurrentStreak = h.Streak?.LastCompletedDate >= targetDate.AddDays(h.Frequency == "Weekly" ? -7 : -1) ? h.Streak.CurrentStreak : 0,
                BestStreak = h.Streak?.BestStreak ?? 0
            }).ToList();

            // --- E. Build Recent Activity (Keep existing) ---
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

                // --- Return Prayer Stats ---
                TotalPrayersToday = prayerTarget,
                PrayersCompletedToday = completedPrayers,

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
