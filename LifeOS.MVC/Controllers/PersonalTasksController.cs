
using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Controllers
{
    public class PersonalTasksController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ProductivityService _productivity;

        public PersonalTasksController(AppDbContext db, ProductivityService productivity)
        {
            _db = db;
            _productivity = productivity;
        }

        // ── INDEX ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var tasks = await _db.PersonalTasks
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToListAsync();
            return View(tasks);
        }

        // ── CREATE ───────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,DueDate,Priority")] PersonalTask task, string? returnUrl)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index");

            task.CreatedAt = DateTime.Now;
            _db.PersonalTasks.Add(task);
            await _db.SaveChangesAsync();

            await _productivity.LogActivityAsync("PersonalTask", task.Id, "Created",
                $"Task '{task.Title}' created");
            TempData["Success"] = $"Task '{task.Title}' added.";
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index");
        }

        // ── EDIT GET ─────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _db.PersonalTasks.FindAsync(id);
            if (task == null) return NotFound();
            return View(task);
        }

        // ── EDIT POST ────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PersonalTask updated)
        {
            var task = await _db.PersonalTasks.FindAsync(updated.Id);
            if (task == null) return NotFound();

            task.Title = updated.Title;
            task.Description = updated.Description;
            task.DueDate = updated.DueDate;
            task.Priority = updated.Priority;

            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("PersonalTask", task.Id, "Updated",
                $"Task '{task.Title}' updated");
            TempData["Success"] = $"Task '{task.Title}' updated.";
            return RedirectToAction("Index");
        }

        // ── TOGGLE COMPLETE ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id, string? returnUrl)
        {
            var task = await _db.PersonalTasks.FindAsync(id);
            if (task == null) return NotFound();

            task.IsCompleted = !task.IsCompleted;
            task.CompletedAt = task.IsCompleted ? DateTime.Now : null;

            await _db.SaveChangesAsync();

            string action = task.IsCompleted ? "Completed" : "Reopened";
            await _productivity.LogActivityAsync("PersonalTask", task.Id, action,
                $"Task '{task.Title}' {action.ToLower()}");

            TempData["Success"] = $"Task '{task.Title}' {action.ToLower()}.";
            if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl!);
            return RedirectToAction("Index");
        }

        // ── DELETE ───────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _db.PersonalTasks.FindAsync(id);
            if (task == null) return NotFound();

            string title = task.Title;
            _db.PersonalTasks.Remove(task);
            await _db.SaveChangesAsync();

            await _productivity.LogActivityAsync("PersonalTask", id, "Deleted",
                $"Task '{title}' deleted");
            TempData["Success"] = $"Task '{title}' removed.";
            return RedirectToAction("Index");
        }
    }
}

