using LifeOS.Models;
using LifeOS.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly Services.ProductivityService _productivity;

        public AnalyticsController(AppDbContext db, Services.ProductivityService productivity)
        {
            _db = db;
            _productivity = productivity;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var last30 = today.AddDays(-29);
            var last7 = today.AddDays(-6);

            // Productivity scores last 7 days
            var scores = new List<(DateTime date, double score)>();
            for (int i = 6; i >= 0; i--)
            {
                var d = today.AddDays(-i);
                var s = await _productivity.GetDailyScoreAsync(d);
                scores.Add((d, s.ProductivityScore));
            }

            // Habit completion last 30 days
            var habitLogs = await _db.HabitLogs
                .Where(l => l.CompletedDate >= last30)
                .GroupBy(l => l.CompletedDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Task completion stats
            var completedTasks = await _db.PersonalTasks.CountAsync(t => t.IsCompleted);
            var openTasks = await _db.PersonalTasks.CountAsync(t => !t.IsCompleted);
            var totalTasks = completedTasks + openTasks;

            // Skill hours last 7 days
            var skillHours = await _db.TechSkillLogs
                .Where(l => l.LogDate >= last7)
                .SumAsync(l => l.Hours);

            // Study hours last 7 days
            var studyHours = await _db.StudySessions
                .Where(s => s.SessionDate >= last7)
                .SumAsync(s => s.DurationHours);

            // Recent reviews
            var reviews = await _db.ReviewEntries
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Habit streaks
            var bestStreaks = await _db.Habits
                .Where(h => h.IsActive)
                .OrderByDescending(h => h.Streak == null ? 0 : h.Streak.CurrentStreak)
                .Take(5)
                .Select(h => new { h.Name, CurrentStreak = h.Streak == null ? 0 : h.Streak.CurrentStreak, BestStreak = h.Streak == null ? 0 : h.Streak.BestStreak })
                .ToListAsync();

            // Activity last 7 days
            var recentActivity = await _db.ActivityLogs
                .OrderByDescending(a => a.DateTime)
                .Take(20)
                .ToListAsync();

            ViewBag.Scores = scores;
            ViewBag.HabitLogs = habitLogs;
            ViewBag.CompletedTasks = completedTasks;
            ViewBag.OpenTasks = openTasks;
            ViewBag.TotalTasks = totalTasks;
            ViewBag.SkillHours = skillHours;
            ViewBag.StudyHours = studyHours;
            ViewBag.Reviews = reviews;
            ViewBag.BestStreaks = bestStreaks;
            ViewBag.Activity = recentActivity;
            ViewBag.AvgScore = scores.Any() ? scores.Average(s => s.score) : 0;
            ViewBag.Today = today;

            return View();
        }
    }
}
