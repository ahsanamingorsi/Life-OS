using LifeOS.Models;
using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
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

            // Task completion stats (one-time tasks)
            var completedTasks = await _db.PersonalTasks.CountAsync(t => t.Repeat == "None" && t.IsCompleted);
            var openTasks = await _db.PersonalTasks.CountAsync(t => t.Repeat == "None" && !t.IsCompleted);
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

            // Repeated task streaks
            var repeatedTasks = await _db.PersonalTasks.AsNoTracking()
                .Include(t => t.Completions)
                .Where(t => t.Repeat != "None")
                .ToListAsync();
            var taskStreaks = repeatedTasks
                .Select(t => new RepeatedTaskStreak
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    Repeat = t.Repeat,
                    CurrentStreak = TaskService.Streak(t, t.Completions.Select(c => c.Date.Date).ToHashSet(), today),
                    Last30Days = t.Completions.Count(c => c.Date >= last30),
                    Total = t.Completions.Count,
                })
                .OrderByDescending(s => s.CurrentStreak).ThenByDescending(s => s.Total)
                .Take(8)
                .ToList();

            // Activity last 7 days
            var recentActivity = await _db.ActivityLogs
                .OrderByDescending(a => a.DateTime)
                .Take(20)
                .ToListAsync();

            ViewBag.Scores = scores;
            ViewBag.CompletedTasks = completedTasks;
            ViewBag.OpenTasks = openTasks;
            ViewBag.TotalTasks = totalTasks;
            ViewBag.SkillHours = skillHours;
            ViewBag.StudyHours = studyHours;
            ViewBag.Reviews = reviews;
            ViewBag.TaskStreaks = taskStreaks;
            ViewBag.Activity = recentActivity;
            ViewBag.AvgScore = scores.Any() ? scores.Average(s => s.score) : 0;
            ViewBag.Today = today;

            return View();
        }
    }
}
