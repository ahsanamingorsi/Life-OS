
using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ProductivityService _productivity;

        private static readonly string[] Motivations =
        {
            "Every expert was once a beginner. Keep building.",
            "Discipline is choosing between what you want now and what you want most.",
            "Your future self is watching. Make him proud.",
            "Small steps every day compound into massive results.",
            "Be the architect of your own destiny.",
            "Sharpen your mind. Strengthen your body. Elevate your spirit.",
            "Progress over perfection. Ship, then improve.",
            "The man who moves a mountain begins by carrying small stones.",
            "Consistency beats intensity every single time.",
            "Do it scared. Do it tired. Just do it."
        };

        public HomeController(AppDbContext db, ProductivityService productivity)
        {
            _db = db;
            _productivity = productivity;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            // ── Productivity engine ──────────────────────────────────
            var prodScore = await _productivity.GetDailyScoreAsync(today);

            // ── All active habits with today's completion status ─────
            var allHabits = prodScore.HabitsWithStreaks;

            // ── Category-specific progress ───────────────────────────
            static int CategoryProgress(
                List<HabitWithStreakViewModel> habits, string category)
            {
                var cat = habits.Where(h =>
                    h.Habit.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (!cat.Any()) return 0;
                int done = cat.Count(h => h.CompletedToday);
                return (int)((double)done / cat.Count * 100);
            }

            // ── Projects ─────────────────────────────────────────────
            var projects = await _db.Projects.ToListAsync();
            int projectsProgress = projects.Any()
                ? (int)projects.Average(p => p.ProgressPercent) : 0;

            // ── Tech skills ──────────────────────────────────────────
            var skills = await _db.TechSkills.ToListAsync();
            int techProgress = skills.Any()
                ? (int)skills.Average(s => s.ProgressPercent) : 0;

            // ── Spiritual ────────────────────────────────────────────
            var todaySpiritual = await _db.SpiritualEntries
                .FirstOrDefaultAsync(s => s.Date == today);
            int spiritualProgress = todaySpiritual != null && todaySpiritual.PrayersTotal > 0
                ? (int)((double)todaySpiritual.PrayersDone / todaySpiritual.PrayersTotal * 100)
                : 0;

            // ── Personal goals ───────────────────────────────────────
            var goals = await _db.PersonalGoals.ToListAsync();
            int personalProgress = goals.Any()
                ? (int)goals.Average(g => g.ProgressPercent) : 0;

            var vm = new DashboardViewModel
            {
                Today = today,
                DailyMotivation = Motivations[today.DayOfYear % Motivations.Length],
                TodaysMission = "Focus on your #1 priority project + 1 hour of deep learning + complete all daily habits.",

                // Stats
                ActiveProjects = await _db.Projects.CountAsync(p => p.Status == "Active"),
                SkillsInProgress = await _db.TechSkills.CountAsync(s => s.IsCurrent),
                CurrentSkill = await _db.TechSkills.FirstOrDefaultAsync(s => s.IsCurrent),
                TopPriorities = await _db.Projects
                                        .Where(p => p.Priority == "A" && p.Status == "Active")
                                        .Take(3).ToListAsync(),
                TodaySpiritual = todaySpiritual,

                // Module progress — all from unified Habit system
                TechProgress = techProgress,
                ProjectsProgress = projectsProgress,
                HealthProgress = CategoryProgress(allHabits, "Health"),
                MindProgress = CategoryProgress(allHabits, "Mind"),
                SpiritualProgress = spiritualProgress,
                PersonalProgress = personalProgress,

                // Productivity engine
                ProductivityScore = prodScore.ProductivityScore,
                TodayHabitsTotal = prodScore.TotalHabits,
                TodayHabitsDone = prodScore.CompletedHabitsToday,
                TotalPersonalTasks = prodScore.TotalPersonalTasks,
                CompletedPersonalTasks = prodScore.CompletedPersonalTasks,
                BestHabitStreak = allHabits.Any() ? allHabits.Max(h => h.BestStreak) : 0,
                CurrentTopStreak = allHabits.Any() ? allHabits.Max(h => h.CurrentStreak) : 0,
                RecentActivityLogs = prodScore.RecentActivity.Take(5).ToList(),
            };

            return View(vm);
        }
    }
}