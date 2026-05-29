
using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ProductivityService _productivity;

        private readonly string[] _motivations = new[]
        {
            "Every expert was once a beginner. Keep building.",
            "Discipline is choosing between what you want now and what you want most.",
            "Your future self is watching. Make him proud.",
            "Small steps every day compound into massive results.",
            "Be the architect of your own destiny.",
            "Sharpen your mind. Strengthen your body. Elevate your spirit.",
            "Progress over perfection. Ship, then improve."
        };

        public HomeController(AppDbContext db, ProductivityService productivity)
        {
            _db = db;
            _productivity = productivity;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            // ── EXISTING LOGIC (unchanged) ───────────────────────────────
            var vm = new DashboardViewModel
            {
                Today = today,
                DailyMotivation = _motivations[today.DayOfYear % _motivations.Length],
                TodaysMission = "Focus on your #1 priority project + 1 hour of deep learning + complete all daily habits.",
                ActiveProjects = await _db.Projects.CountAsync(p => p.Status == "Active"),
                SkillsInProgress = await _db.TechSkills.CountAsync(s => s.IsCurrent),
                CurrentSkill = await _db.TechSkills.FirstOrDefaultAsync(s => s.IsCurrent),
                TopPriorities = await _db.Projects.Where(p => p.Priority == "A" && p.Status == "Active").Take(3).ToListAsync(),
                TodayHealth = await _db.HealthEntries.FirstOrDefaultAsync(h => h.Date == today),
                TodaySpiritual = await _db.SpiritualEntries.FirstOrDefaultAsync(s => s.Date == today),
            };

            var habit = await _db.HabitEntries.FirstOrDefaultAsync(h => h.Date == today);
            if (habit != null)
            {
                int done = 0;
                if (habit.CommunicationPractice) done++;
                if (habit.CreativityTask) done++;
                if (habit.LogicTask) done++;
                if (habit.PagesRead > 0) done++;
                vm.TodayHabitsCompleted = done;
            }
            vm.TotalHabits = 4;

            var skills = await _db.TechSkills.ToListAsync();
            vm.TechProgress = skills.Any() ? (int)skills.Average(s => s.ProgressPercent) : 0;

            var projects = await _db.Projects.ToListAsync();
            vm.ProjectsProgress = projects.Any() ? (int)projects.Average(p => p.ProgressPercent) : 0;

            if (vm.TodayHealth != null)
            {
                int h = 0;
                if (vm.TodayHealth.WorkoutDone) h += 25;
                if (vm.TodayHealth.WaterLiters >= 2) h += 25;
                if (vm.TodayHealth.SleepHours >= 7) h += 25;
                if (vm.TodayHealth.DietClean) h += 25;
                vm.HealthProgress = h;
            }

            if (vm.TodaySpiritual != null && vm.TodaySpiritual.PrayersTotal > 0)
                vm.SpiritualProgress = (int)((double)vm.TodaySpiritual.PrayersDone / vm.TodaySpiritual.PrayersTotal * 100);

            if (habit != null)
                vm.MindProgress = vm.TotalHabits > 0 ? (int)((double)vm.TodayHabitsCompleted / vm.TotalHabits * 100) : 0;

            var goals = await _db.PersonalGoals.ToListAsync();
            vm.PersonalProgress = goals.Any() ? (int)goals.Average(g => g.ProgressPercent) : 0;

            // ── NEW: PRODUCTIVITY MODULE ─────────────────────────────────
            var prodScore = await _productivity.GetDailyScoreAsync(today);

            vm.ProductivityScore = prodScore.ProductivityScore;
            vm.TodayHabitsTotal = prodScore.TotalHabits;
            vm.TodayHabitsDone = prodScore.CompletedHabitsToday;
            vm.TotalPersonalTasks = prodScore.TotalPersonalTasks;
            vm.CompletedPersonalTasks = prodScore.CompletedPersonalTasks;
            vm.BestHabitStreak = prodScore.HabitsWithStreaks.Any()
                                            ? prodScore.HabitsWithStreaks.Max(h => h.BestStreak) : 0;
            vm.CurrentTopStreak = prodScore.HabitsWithStreaks.Any()
                                            ? prodScore.HabitsWithStreaks.Max(h => h.CurrentStreak) : 0;
            vm.RecentActivityLogs = prodScore.RecentActivity.Take(5).ToList();

            return View(vm);
        }
    }
}