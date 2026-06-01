using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    /// <summary>
    /// Health module — shows habits filtered to category "Health".
    /// All CRUD goes through HabitsController; this controller only
    /// assembles the category view model.
    /// </summary>
    public class HealthController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ProductivityService _productivity;

        public HealthController(AppDbContext db, ProductivityService productivity)
        {
            _db = db;
            _productivity = productivity;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var vm = await BuildViewModelAsync("Health",
                "Health & Body",
                "Track your physical habits. Your body is the foundation of everything.");
            return View(vm);
        }

        // ── Shared builder — reused by Health and any future category page ──
        internal async Task<HabitCategoryViewModel> BuildViewModelAsync(
            string category, string title, string subtitle)
        {
            var today = DateTime.Today;

            var habits = await _db.Habits
                .Where(h => h.IsActive &&
                       h.Category.ToLower() == category.ToLower())
                .Include(h => h.HabitLogs)
                .Include(h => h.Streak)
                .OrderBy(h => h.Name)
                .ToListAsync();

            var completedTodayIds = await _db.HabitLogs
                .Where(l => l.CompletedDate.Date == today)
                .Select(l => l.HabitId)
                .ToListAsync();

            // Last 7 days completion heatmap
            var last7Start = today.AddDays(-6);
            var last7Logs = await _db.HabitLogs
                .Where(l => l.CompletedDate.Date >= last7Start &&
                            habits.Select(h => h.Id).Contains(l.HabitId))
                .ToListAsync();

            var last7Dict = new Dictionary<DateTime, int>();
            for (int i = 0; i < 7; i++)
            {
                var d = last7Start.AddDays(i);
                last7Dict[d] = last7Logs.Count(l => l.CompletedDate.Date == d);
            }

            var habitsWithStreaks = habits.Select(h => new HabitWithStreakViewModel
            {
                Habit = h,
                CompletedToday = completedTodayIds.Contains(h.Id),
                CurrentStreak = h.Streak?.CurrentStreak ?? 0,
                BestStreak = h.Streak?.BestStreak ?? 0,
            }).ToList();

            return new HabitCategoryViewModel
            {
                Category = category,
                PageTitle = title,
                PageSubtitle = subtitle,
                Habits = habitsWithStreaks,
                Last7DaysCompletion = last7Dict,
            };
        }
    }
}