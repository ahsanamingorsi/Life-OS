using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Controllers
{
    public class PersonalTasksController : Controller
    {
        private static readonly string[] Filters = { "open", "today", "repeated", "overdue", "done", "all" };

        private readonly AppDbContext _db;
        private readonly ProductivityService _productivity;
        private readonly TaskService _tasks;

        public PersonalTasksController(AppDbContext db, ProductivityService productivity, TaskService tasks)
        {
            _db = db;
            _productivity = productivity;
            _tasks = tasks;
        }

        // ── INDEX: one page, filtered ────────────────────────────────────
        public async Task<IActionResult> Index(string? filter, int? goalId)
        {
            var today = DateTime.Today;
            filter = Filters.Contains(filter) ? filter! : "open";

            var all = await _db.PersonalTasks.AsNoTracking()
                .Include(t => t.Goal)
                .Include(t => t.Completions)
                .ToListAsync();

            var rows = all.Select(t =>
            {
                var done = t.Completions.Select(c => c.Date.Date).ToHashSet();
                var row = new TaskRow
                {
                    Task = t,
                    RepeatText = TaskService.RepeatText(t),
                    OccursToday = TaskService.OccursOn(t, today),
                    DoneToday = t.IsRepeated && done.Contains(today),
                    IsOverdue = !t.IsRepeated && !t.IsCompleted && t.DueDate < today,
                    Streak = TaskService.Streak(t, done, today),
                    TimesDone = done.Count,
                };
                if (t.IsRepeated)
                {
                    for (var d = today; d <= today.AddDays(400); d = d.AddDays(1))
                        if (TaskService.OccursOn(t, d) && !(d == today && row.DoneToday)) { row.NextDate = d; break; }
                }
                return row;
            }).ToList();

            bool IsFinished(TaskRow r) => r.Task.IsRepeated ? r.NextDate == null : r.Task.IsCompleted;

            var vm = new TasksPageViewModel
            {
                Today = today,
                Filter = filter,
                GoalId = goalId,
                Goals = await _db.PersonalGoals.AsNoTracking().OrderBy(g => g.Goal).ToListAsync(),
                OpenCount = rows.Count(r => !IsFinished(r)),
                TodayCount = rows.Count(r => r.OccursToday),
                TodayDone = rows.Count(r => r.OccursToday && (r.Task.IsRepeated ? r.DoneToday : r.Task.IsCompleted)),
                RepeatedCount = rows.Count(r => r.Task.IsRepeated && !IsFinished(r)),
                OverdueCount = rows.Count(r => r.IsOverdue),
                DoneCount = rows.Count(r => !r.Task.IsRepeated && r.Task.IsCompleted),
            };

            IEnumerable<TaskRow> shown = filter switch
            {
                "today" => rows.Where(r => r.OccursToday || r.IsOverdue),
                "repeated" => rows.Where(r => r.Task.IsRepeated),
                "overdue" => rows.Where(r => r.IsOverdue),
                "done" => rows.Where(r => !r.Task.IsRepeated && r.Task.IsCompleted),
                "all" => rows,
                _ => rows.Where(r => !IsFinished(r)),
            };
            if (goalId.HasValue) shown = shown.Where(r => r.Task.GoalId == goalId);

            vm.Rows = shown
                .OrderBy(r => IsFinished(r))
                .ThenByDescending(r => r.IsOverdue)
                .ThenByDescending(r => r.OccursToday && !r.DoneToday && !(r.Task.IsCompleted))
                .ThenBy(r => r.Task.Priority)
                .ThenBy(r => r.Task.IsRepeated ? r.NextDate : r.Task.DueDate ?? DateTime.MaxValue)
                .ThenByDescending(r => r.Task.CompletedAt)
                .ToList();

            return View(vm);
        }

        // ── CREATE ───────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create(int? goalId, string? returnUrl)
        {
            var task = new PersonalTask { DueDate = DateTime.Today, GoalId = goalId };
            return View("Edit", await FormAsync(task, returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Description,DueDate,Priority,Repeat,RepeatDayOfMonth,RepeatEndDate,GoalId")] PersonalTask task,
            int[]? repeatDay, string? returnUrl)
        {
            var error = await NormalizeAsync(task, repeatDay);
            if (error != null)
            {
                TempData["Error"] = error;
                return SafeRedirect(returnUrl, Url.Action("Create", new { returnUrl })!);
            }

            task.CreatedAt = DateTime.Now;
            _db.PersonalTasks.Add(task);
            await _db.SaveChangesAsync();

            await _productivity.LogActivityAsync("PersonalTask", task.Id, "Created", $"Task '{task.Title}' created");
            TempData["Success"] = $"Task '{task.Title}' added.";
            return SafeRedirect(returnUrl, "/PersonalTasks");
        }

        // ── EDIT ─────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? returnUrl)
        {
            var task = await _db.PersonalTasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();
            return View(await FormAsync(task, returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [Bind("Id,Title,Description,DueDate,Priority,Repeat,RepeatDayOfMonth,RepeatEndDate,GoalId")] PersonalTask updated,
            int[]? repeatDay, string? returnUrl)
        {
            var task = await _db.PersonalTasks.FindAsync(updated.Id);
            if (task == null) return NotFound();

            var error = await NormalizeAsync(updated, repeatDay);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Edit", new { id = updated.Id, returnUrl });
            }

            bool wasRepeated = task.IsRepeated;
            task.Title = updated.Title;
            task.Description = updated.Description;
            task.DueDate = updated.DueDate;
            task.Priority = updated.Priority;
            task.Repeat = updated.Repeat;
            task.RepeatDays = updated.RepeatDays;
            task.RepeatDayOfMonth = updated.RepeatDayOfMonth;
            task.RepeatEndDate = updated.RepeatEndDate;
            task.GoalId = updated.GoalId;

            // Switching from repeated to one-time: the day-by-day history no longer applies.
            if (wasRepeated && !task.IsRepeated)
                _db.TaskCompletions.RemoveRange(_db.TaskCompletions.Where(c => c.PersonalTaskId == task.Id));
            // Switching from one-time to repeated: completion is now tracked per day.
            if (!wasRepeated && task.IsRepeated) { task.IsCompleted = false; task.CompletedAt = null; }

            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("PersonalTask", task.Id, "Updated", $"Task '{task.Title}' updated");
            TempData["Success"] = $"Task '{task.Title}' updated.";
            return SafeRedirect(returnUrl, "/PersonalTasks");
        }

        // ── TOGGLE COMPLETE (one-time, or one day of a repeated task) ────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id, DateTime? date, string? returnUrl)
        {
            var title = await _db.PersonalTasks.Where(t => t.Id == id).Select(t => t.Title).FirstOrDefaultAsync();
            if (title == null) return NotFound();

            var state = await _tasks.ToggleAsync(id, date);
            if (state == null)
            {
                TempData["Error"] = "This task does not repeat on that day, or the day is in the future.";
                return SafeRedirect(returnUrl, "/PersonalTasks");
            }

            string action = state.Value ? "Completed" : "Reopened";
            await _productivity.LogActivityAsync("PersonalTask", id, action, $"Task '{title}' {action.ToLower()}");
            TempData["Success"] = $"Task '{title}' {action.ToLower()}.";
            return SafeRedirect(returnUrl, "/PersonalTasks");
        }

        // ── DELETE ───────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl)
        {
            var task = await _db.PersonalTasks.FindAsync(id);
            if (task == null) return NotFound();

            string title = task.Title;
            _db.PersonalTasks.Remove(task); // completions cascade
            await _db.SaveChangesAsync();

            await _productivity.LogActivityAsync("PersonalTask", id, "Deleted", $"Task '{title}' deleted");
            TempData["Success"] = $"Task '{title}' removed.";
            return SafeRedirect(returnUrl, "/PersonalTasks");
        }

        // ── HELPERS ──────────────────────────────────────────────────────
        private async Task<TaskFormViewModel> FormAsync(PersonalTask task, string? returnUrl) => new()
        {
            Task = task,
            Goals = await _db.PersonalGoals.AsNoTracking().OrderBy(g => g.IsAchieved).ThenBy(g => g.Goal).ToListAsync(),
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null,
        };

        /// <summary>Checks and cleans the repeat fields. Returns an error message, or null when valid.</summary>
        private async Task<string?> NormalizeAsync(PersonalTask task, int[]? repeatDay)
        {
            task.Title = (task.Title ?? "").Trim();
            if (task.Title.Length == 0) return "Enter a task name.";
            if (task.GoalId.HasValue && !await _db.PersonalGoals.AnyAsync(g => g.Id == task.GoalId)) return "The chosen goal no longer exists.";

            switch (task.Repeat)
            {
                case "None":
                    task.RepeatDays = 0;
                    task.RepeatDayOfMonth = null;
                    task.RepeatEndDate = null;
                    return null;
                case "Daily":
                case "Weekly":
                case "Monthly":
                    task.DueDate = (task.DueDate ?? DateTime.Today).Date;
                    if (task.RepeatEndDate.HasValue && task.RepeatEndDate.Value.Date < task.DueDate)
                        return "The end date must be on or after the start date.";
                    task.RepeatDays = 0;
                    if (task.Repeat == "Weekly")
                    {
                        task.RepeatDays = (repeatDay ?? Array.Empty<int>())
                            .Where(b => TaskService.WeekDays.Any(w => w.Bit == b))
                            .Aggregate(0, (sum, b) => sum | b);
                        if (task.RepeatDays == 0) return "Choose at least one day of the week.";
                    }
                    if (task.Repeat == "Monthly")
                    {
                        task.RepeatDayOfMonth ??= task.DueDate.Value.Day;
                        if (task.RepeatDayOfMonth is < 1 or > 31) return "Choose a day of the month between 1 and 31.";
                    }
                    else task.RepeatDayOfMonth = null;
                    return null;
                default:
                    return "Choose how the task repeats.";
            }
        }

        private IActionResult SafeRedirect(string? returnUrl, string fallback) =>
            Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : LocalRedirect(fallback);
    }
}
