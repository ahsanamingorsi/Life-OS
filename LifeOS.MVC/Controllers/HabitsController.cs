
using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Controllers
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

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var score = await _productivity.GetDailyScoreAsync();
            return View(score);
        }

        // ── CREATE ───────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Habit habit)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index");

            habit.CreatedAt = DateTime.Now;
            habit.IsActive = true;
            _db.Habits.Add(habit);
            await _db.SaveChangesAsync();

            // Seed an empty streak row immediately
            _db.Streaks.Add(new Streak { HabitId = habit.Id });
            await _db.SaveChangesAsync();

            await _productivity.LogActivityAsync("Habit", habit.Id, "Created", $"Habit '{habit.Name}' created");
            TempData["Success"] = $"Habit '{habit.Name}' added.";
            return RedirectToAction("Index");
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var habit = await _db.Habits.FindAsync(id);
            if (habit == null) return NotFound();
            return PartialView("_EditHabitModal", habit);
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Habit updated)
        {
            var habit = await _db.Habits.FindAsync(updated.Id);
            if (habit == null) return NotFound();

            habit.Name = updated.Name;
            habit.Description = updated.Description;
            habit.Frequency = updated.Frequency;
            habit.Category = updated.Category;
            habit.IsActive = updated.IsActive;

            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("Habit", habit.Id, "Updated", $"Habit '{habit.Name}' updated");
            TempData["Success"] = $"Habit '{habit.Name}' updated.";
            return RedirectToAction("Index");
        }

        // ── DELETE ───────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var habit = await _db.Habits.FindAsync(id);
            if (habit == null) return NotFound();

            string name = habit.Name;
            _db.Habits.Remove(habit); // cascade deletes logs + streak
            await _db.SaveChangesAsync();

            await _productivity.LogActivityAsync("Habit", id, "Deleted", $"Habit '{name}' deleted");
            TempData["Success"] = $"Habit '{name}' removed.";
            return RedirectToAction("Index");
        }

        // ── MARK COMPLETE ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int habitId, string? note)
        {
            var habit = await _db.Habits.FindAsync(habitId);
            if (habit == null) return NotFound();

            var today = DateTime.Today;
            bool alreadyDone = await _db.HabitLogs
                .AnyAsync(l => l.HabitId == habitId && l.CompletedDate.Date == today);

            if (!alreadyDone)
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
                    $"Habit '{habit.Name}' completed for {today:MMM d}");

                TempData["Success"] = $"'{habit.Name}' marked complete!";
            }
            else
            {
                TempData["Info"] = $"'{habit.Name}' already completed today.";
            }

            return RedirectToAction("Index");
        }

        // ── UNMARK COMPLETE ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnmarkComplete(int habitId)
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
                    $"Habit '{habit?.Name}' unmarked for {today:MMM d}");
                TempData["Info"] = "Habit unmarked.";
            }

            return RedirectToAction("Index");
        }

        // ── HISTORY ──────────────────────────────────────────────────────
        public async Task<IActionResult> History(int id)
        {
            var habit = await _db.Habits
                .Include(h => h.HabitLogs)
                .Include(h => h.Streak)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (habit == null) return NotFound();
            return View(habit);
        }
    }
}