
using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ProductivityService _productivity;

        public ProjectsController(AppDbContext db, ProductivityService productivity)
        {
            _db = db;
            _productivity = productivity;
        }

        // ── PROJECT LIST ─────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var projects = await _db.Projects
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.Deadline)
                .ToListAsync();

            // Attach task counts without loading all task data
            var projectIds = projects.Select(p => p.Id).ToList();
            var taskCounts = await _db.ProjectTasks
                .Where(t => projectIds.Contains(t.ProjectId))
                .GroupBy(t => t.ProjectId)
                .Select(g => new
                {
                    ProjectId = g.Key,
                    Total = g.Count(),
                    Done = g.Count(t => t.IsCompleted)
                })
                .ToListAsync();

            ViewBag.TaskCounts = taskCounts.ToDictionary(
                x => x.ProjectId,
                x => new ProjectTaskCount(x.Total, x.Done));
            ViewBag.Categories = await CategorySuggestionsAsync();

            return View(projects);
        }

        // Suggestions for the free-text category box: the old fixed list plus every category in use.
        private static readonly string[] DefaultCategories = { "EduBridgePath", "PSC", "Farming", "Future Ideas", "Personal", "Tech", "Study" };

        private async Task<List<string>> CategorySuggestionsAsync()
        {
            var used = await _db.Projects.Where(p => p.Category != "").Select(p => p.Category).Distinct().ToListAsync();
            return DefaultCategories.Concat(used).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c).ToList();
        }

        // ── PROJECT DETAIL + TASKS ───────────────────────────────────
        public async Task<IActionResult> Detail(int id)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var tasks = await _db.ProjectTasks
                .Where(t => t.ProjectId == id)
                .OrderBy(t => t.Status)
                .ThenBy(t => t.SortOrder)
                .ThenBy(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToListAsync();

            var vm = new ProjectDetailViewModel
            {
                Project = project,
                Tasks = tasks
            };
            ViewBag.Categories = await CategorySuggestionsAsync();

            return View(vm);
        }

        // ── CREATE PROJECT ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Title,Category,Status,Priority,ProgressPercent,NextTask,Notes,Deadline")] Project project)
        {
            project.Title = (project.Title ?? "").Trim();
            project.Category = (project.Category ?? "").Trim();
            if (project.Title.Length == 0) { TempData["Error"] = "Enter a project title."; return RedirectToAction("Index"); }
            if (project.Priority is not ("A" or "B" or "C") || project.Status is not ("Active" or "Paused"))
            { TempData["Error"] = "Choose a valid priority and status."; return RedirectToAction("Index"); }
            project.ProgressPercent = Math.Clamp(project.ProgressPercent, 0, 100);
            project.CreatedAt = DateTime.Now;
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("Project", project.Id, "Created",
                $"Project '{project.Title}' created");
            TempData["Success"] = $"Project '{project.Title}' created.";
            return RedirectToAction("Detail", new { id = project.Id });
        }

        // ── UPDATE PROJECT ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, int? progress, string? nextTask,
            string? title, string? category, string? priority, DateTime? deadline, string? notes)
        {
            var proj = await _db.Projects.FindAsync(id);
            if (proj == null) return NotFound();

            if (!new[] { "Active", "Paused", "Done" }.Contains(status)) return BadRequest("Invalid project status.");
            title = title?.Trim();
            if (string.IsNullOrEmpty(title) || title.Length > 150) { TempData["Error"] = "Enter a project title (up to 150 characters)."; return RedirectToAction("Detail", new { id }); }
            if (priority is not ("A" or "B" or "C")) { TempData["Error"] = "Choose priority A, B or C."; return RedirectToAction("Detail", new { id }); }
            if (category?.Length > 50 || notes?.Length > 500 || nextTask?.Length > 300) { TempData["Error"] = "One of the fields is too long."; return RedirectToAction("Detail", new { id }); }

            proj.Title = title;
            proj.Category = category?.Trim() ?? "";
            proj.Priority = priority;
            proj.Deadline = deadline?.Date;
            proj.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            proj.Status = status;
            // Progress is typed by hand only when the project has no tasks; otherwise tasks decide it.
            bool hasTasks = await _db.ProjectTasks.AnyAsync(t => t.ProjectId == id);
            if (!hasTasks && progress.HasValue) proj.ProgressPercent = Math.Clamp(progress.Value, 0, 100);
            if (nextTask != null) proj.NextTask = nextTask;

            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("Project", id, "Updated",
                $"Project '{proj.Title}' updated to {status}");
            TempData["Success"] = "Project updated.";
            return RedirectToAction("Detail", new { id });
        }

        // ── DELETE PROJECT ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var proj = await _db.Projects.FindAsync(id);
            if (proj == null) return NotFound();

            string title = proj.Title;
            _db.Projects.Remove(proj); // cascades to ProjectTasks
            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("Project", id, "Deleted",
                $"Project '{title}' deleted");
            TempData["Success"] = $"'{title}' deleted.";
            return RedirectToAction("Index");
        }

        // ══════════════════════════════════════════════════════════════
        // TASK ACTIONS
        // ══════════════════════════════════════════════════════════════

        // ── ADD TASK ─────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTask([Bind("ProjectId,Title,Description,Priority,DueDate,EstimatedMinutes,Notes,SortOrder")] ProjectTask task)
        {
            if (!await _db.Projects.AnyAsync(p => p.Id == task.ProjectId)) return NotFound();
            var error = CheckTask(task);
            if (error != null) { TempData["Error"] = error; return RedirectToAction("Detail", new { id = task.ProjectId }); }
            task.Title = task.Title.Trim();
            task.CreatedAt = DateTime.Now;
            task.Status = "Todo";
            _db.ProjectTasks.Add(task);
            await _db.SaveChangesAsync();
            await AutoUpdateProjectProgress(task.ProjectId);
            await _productivity.LogActivityAsync("ProjectTask", task.Id, "Created",
                $"Task '{task.Title}' added to project");
            TempData["Success"] = $"Task '{task.Title}' added.";
            return RedirectToAction("Detail", new { id = task.ProjectId });
        }

        // ── MOVE TASK STATUS ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveTask(int taskId, string status)
        {
            var task = await _db.ProjectTasks.FindAsync(taskId);
            if (task == null) return NotFound();

            if (!new[] { "Todo", "InProgress", "Done" }.Contains(status)) return BadRequest("Invalid task status.");
            task.Status = status;
            if (status == "Done")
            {
                task.IsCompleted = true;
                task.CompletedAt = DateTime.Now;
            }
            else
            {
                task.IsCompleted = false;
                task.CompletedAt = null;
            }

            await _db.SaveChangesAsync();
            await AutoUpdateProjectProgress(task.ProjectId);
            await _productivity.LogActivityAsync("ProjectTask", task.Id,
                status == "Done" ? "Completed" : "Updated",
                $"Task '{task.Title}' moved to {status}");

            TempData["Success"] = $"'{task.Title}' → {status}.";
            return RedirectToAction("Detail", new { id = task.ProjectId });
        }

        // ── EDIT TASK ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask(ProjectTask updated)
        {
            var task = await _db.ProjectTasks.FindAsync(updated.Id);
            if (task == null) return NotFound();
            var error = CheckTask(updated);
            if (error != null) { TempData["Error"] = error; return RedirectToAction("Detail", new { id = task.ProjectId }); }

            task.Title = updated.Title.Trim();
            task.Description = updated.Description;
            task.Priority = updated.Priority;
            task.DueDate = updated.DueDate;
            task.EstimatedMinutes = updated.EstimatedMinutes;
            task.ActualMinutes = updated.ActualMinutes;
            task.Notes = updated.Notes;

            await _db.SaveChangesAsync();
            // Title, priority and due date decide the project's "next task", so refresh it.
            await AutoUpdateProjectProgress(task.ProjectId);
            await _productivity.LogActivityAsync("ProjectTask", task.Id, "Updated",
                $"Task '{task.Title}' updated");
            TempData["Success"] = $"'{task.Title}' updated.";
            return RedirectToAction("Detail", new { id = task.ProjectId });
        }

        // ── DELETE TASK ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int taskId, int projectId)
        {
            var task = await _db.ProjectTasks.FindAsync(taskId);
            if (task == null) return NotFound();

            projectId = task.ProjectId;
            string title = task.Title;
            _db.ProjectTasks.Remove(task);
            await _db.SaveChangesAsync();
            await AutoUpdateProjectProgress(projectId);
            await _productivity.LogActivityAsync("ProjectTask", taskId, "Deleted",
                $"Task '{title}' deleted");
            TempData["Success"] = $"Task '{title}' removed.";
            return RedirectToAction("Detail", new { id = projectId });
        }

        // ── LOG TIME ─────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogTime(int taskId, int minutes)
        {
            var task = await _db.ProjectTasks.FindAsync(taskId);
            if (task == null) return NotFound();

            if (minutes <= 0 || minutes > 1440) { TempData["Error"] = "Enter between 1 and 1440 minutes."; return RedirectToAction("Detail", new { id = task.ProjectId }); }
            task.ActualMinutes = checked((task.ActualMinutes ?? 0) + minutes);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{minutes} min logged on '{task.Title}'.";
            return RedirectToAction("Detail", new { id = task.ProjectId });
        }

        /// <summary>Title, priority and minutes checks for a project task. Returns an error message or null.</summary>
        private static string? CheckTask(ProjectTask t)
        {
            if (string.IsNullOrWhiteSpace(t.Title)) return "Enter a task title.";
            if (t.Priority is not ("A" or "B" or "C")) return "Choose priority A, B or C.";
            if (t.EstimatedMinutes is < 0 or > 100000 || t.ActualMinutes is < 0 or > 100000) return "Minutes must be between 0 and 100000.";
            return null;
        }

        // ── AUTO-SYNC PROJECT PROGRESS ───────────────────────────────
        private async Task AutoUpdateProjectProgress(int projectId)
        {
            var project = await _db.Projects.FindAsync(projectId);
            if (project == null) return;

            var tasks = await _db.ProjectTasks
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            if (tasks.Any())
            {
                int done = tasks.Count(t => t.IsCompleted);
                project.ProgressPercent = (int)((double)done / tasks.Count * 100);

                // Auto-set status
                if (project.ProgressPercent == 100)
                    project.Status = "Done";
                else if (project.Status == "Done")
                    project.Status = "Active";

                // Auto-set next task
                var nextTask = tasks
                    .Where(t => !t.IsCompleted)
                    .OrderBy(t => t.Priority)
                    .ThenBy(t => t.DueDate)
                    .FirstOrDefault();
                project.NextTask = nextTask?.Title;
            }

            if (!tasks.Any()) { project.ProgressPercent = 0; project.NextTask = null; if (project.Status == "Done") project.Status = "Active"; }
            await _db.SaveChangesAsync();
        }
    }
}


