using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class HabitsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly StreakService _streak;
        private readonly ProductivityService _productivity;

        public HabitsController(AppDbContext db, StreakService streak, ProductivityService productivity)
        {
            _db = db;
            _streak = streak;
            _productivity = productivity;
        }

        // ── INDEX ────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var score = await _productivity.GetDailyScoreAsync();
            return View(score);
        }

        // ── CREATE ───────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Habit habit, string? returnUrl)
        {
            habit.CreatedAt = DateTime.Now;
            habit.IsActive = true;
            _db.Habits.Add(habit);
            await _db.SaveChangesAsync();

            _db.Streaks.Add(new Streak { HabitId = habit.Id });
            await _db.SaveChangesAsync();

            await _productivity.LogActivityAsync("Habit", habit.Id, "Created",
                $"Habit '{habit.Name}' created");

            TempData["Success"] = $"Habit '{habit.Name}' added.";
            return Redirect(GetSafeReturnUrl(returnUrl));
        }

        // ── EDIT GET ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? returnUrl)
        {
            var habit = await _db.Habits.FindAsync(id);
            if (habit == null) return NotFound();
            ViewBag.ReturnUrl = returnUrl ?? "/Habits";
            return View(habit);
        }

        // ── EDIT POST ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Habit updated, string? returnUrl)
        {
            var habit = await _db.Habits.FindAsync(updated.Id);
            if (habit == null) return NotFound();

            habit.Name = updated.Name;
            habit.Description = updated.Description;
            habit.Frequency = updated.Frequency;
            habit.Category = updated.Category;
            habit.IsActive = updated.IsActive;

            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("Habit", habit.Id, "Updated",
                $"Habit '{habit.Name}' updated");

            TempData["Success"] = $"'{habit.Name}' updated.";
            return Redirect(GetSafeReturnUrl(returnUrl));
        }

        // ── DELETE ───────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl)
        {
            var habit = await _db.Habits.FindAsync(id);
            if (habit == null) return NotFound();

            string name = habit.Name;
            _db.Habits.Remove(habit);
            await _db.SaveChangesAsync();

            await _productivity.LogActivityAsync("Habit", id, "Deleted",
                $"Habit '{name}' deleted");

            TempData["Success"] = $"'{name}' removed.";
            return Redirect(GetSafeReturnUrl(returnUrl));
        }

        // ── MARK COMPLETE ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int habitId, string? note, string? returnUrl)
        {
            var habit = await _db.Habits.FindAsync(habitId);
            if (habit == null) return NotFound();

            var today = DateTime.Today;
            bool already = await _db.HabitLogs
                .AnyAsync(l => l.HabitId == habitId && l.CompletedDate.Date == today);

            if (!already)
            {
                _db.HabitLogs.Add(new HabitLog
                {
                    HabitId = habitId,
                    CompletedDate = today,
                    Note = note,
                    LoggedAt = DateTime.Now
                });
                await _db.SaveChangesAsync();
                await _streak.RecalculateAsync(habitId);
                await _productivity.LogActivityAsync("Habit", habitId, "Completed",
                    $"'{habit.Name}' completed");
                TempData["Success"] = $"'{habit.Name}' marked complete!";
            }
            else
            {
                TempData["Info"] = $"'{habit.Name}' already done today.";
            }

            return Redirect(GetSafeReturnUrl(returnUrl));
        }

        // ── UNMARK COMPLETE ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnmarkComplete(int habitId, string? returnUrl)
        {
            var today = DateTime.Today;
            var log = await _db.HabitLogs
                .FirstOrDefaultAsync(l => l.HabitId == habitId && l.CompletedDate.Date == today);

            if (log != null)
            {
                var habit = await _db.Habits.FindAsync(habitId);
                _db.HabitLogs.Remove(log);
                await _db.SaveChangesAsync();
                await _streak.RecalculateAsync(habitId);
                await _productivity.LogActivityAsync("Habit", habitId, "Uncompleted",
                    $"'{habit?.Name}' unmarked");
                TempData["Info"] = "Habit unmarked.";
            }

            return Redirect(GetSafeReturnUrl(returnUrl));
        }

        // ── HISTORY ──────────────────────────────────────────────────
        public async Task<IActionResult> History(int id)
        {
            var habit = await _db.Habits
                .Include(h => h.HabitLogs)
                .Include(h => h.Streak)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (habit == null) return NotFound();
            return View(habit);
        }

        // ── HELPER ───────────────────────────────────────────────────
        private string GetSafeReturnUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return "/Habits";
            // Only allow relative URLs to prevent open redirect
            if (Url.IsLocalUrl(url)) return url;
            return "/Habits";
        }
    }
}
