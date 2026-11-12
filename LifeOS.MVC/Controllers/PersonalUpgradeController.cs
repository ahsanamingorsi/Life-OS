using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    /// <summary>Goals. Progress comes from linked tasks; manual progress only when none are linked.</summary>
    public class PersonalUpgradeController : Controller
    {
        public static readonly string[] Categories = { "Grooming", "Confidence", "Lifestyle", "Standard" };

        private readonly AppDbContext _db;
        private readonly ProductivityService _productivity;

        public PersonalUpgradeController(AppDbContext db, ProductivityService productivity)
        {
            _db = db;
            _productivity = productivity;
        }

        public async Task<IActionResult> Index()
        {
            var goals = await _db.PersonalGoals.AsNoTracking()
                .Include(g => g.Tasks).ThenInclude(t => t.Completions)
                .OrderBy(g => g.IsAchieved).ThenBy(g => g.Category).ThenBy(g => g.Goal)
                .ToListAsync();

            var today = DateTime.Today;
            var cards = goals.Select(g =>
            {
                var oneTime = g.Tasks.Where(t => !t.IsRepeated).ToList();
                return new GoalCard
                {
                    Goal = g,
                    Progress = TaskService.GoalProgress(g),
                    IsAuto = g.Tasks.Any(),
                    OneTimeDone = oneTime.Count(t => t.IsCompleted),
                    OneTimeTotal = oneTime.Count,
                    RepeatedCount = g.Tasks.Count(t => t.IsRepeated),
                    Completions = TaskService.GoalCompletions(g),
                    Tasks = g.Tasks
                        .OrderBy(t => t.IsRepeated ? 0 : 1).ThenBy(t => t.IsCompleted).ThenBy(t => t.Title)
                        .Select(t => new GoalTaskLine
                        {
                            Task = t,
                            Text = t.IsRepeated
                                ? $"{TaskService.RepeatText(t)} · done {t.Completions.Count}×"
                                : t.IsCompleted ? "Done" : t.DueDate.HasValue ? (t.DueDate < today ? "Overdue · " : "Due ") + t.DueDate.Value.ToString("dd MMM") : "No due date",
                            IsDone = t.IsRepeated ? t.Completions.Any(c => c.Date == today) : t.IsCompleted,
                        }).ToList(),
                };
            }).ToList();

            ViewBag.Categories = Categories;
            return View(cards);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Goal,Category,ProgressPercent,TargetCount,Notes")] PersonalGoal entry)
        {
            entry.Goal = entry.Goal.Trim();
            if (entry.Goal.Length == 0) { TempData["Error"] = "Enter a goal."; return RedirectToAction("Index"); }
            entry.ProgressPercent = Math.Clamp(entry.ProgressPercent, 0, 100);
            entry.CreatedAt = DateTime.Now;
            _db.PersonalGoals.Add(entry);
            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("Goal", entry.Id, "Created", $"Goal '{entry.Goal}' created");
            TempData["Success"] = $"Goal '{entry.Goal}' added.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Goal,Category,TargetCount,Notes")] PersonalGoal updated)
        {
            var goal = await _db.PersonalGoals.FindAsync(updated.Id);
            if (goal == null) return NotFound();
            updated.Goal = updated.Goal.Trim();
            if (updated.Goal.Length == 0) { TempData["Error"] = "Enter a goal."; return RedirectToAction("Index"); }

            goal.Goal = updated.Goal;
            goal.Category = updated.Category;
            goal.TargetCount = updated.TargetCount;
            goal.Notes = updated.Notes;
            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("Goal", goal.Id, "Updated", $"Goal '{goal.Goal}' updated");
            TempData["Success"] = "Goal updated.";
            return RedirectToAction("Index");
        }

        /// <summary>Manual progress (only used when no task is linked) and the achieved switch.</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(int id, int? progress, bool achieved)
        {
            var goal = await _db.PersonalGoals.Include(g => g.Tasks).FirstOrDefaultAsync(g => g.Id == id);
            if (goal == null) return NotFound();

            if (progress.HasValue && !goal.Tasks.Any())
                goal.ProgressPercent = Math.Clamp(progress.Value, 0, 100);
            bool newlyAchieved = achieved && !goal.IsAchieved;
            goal.IsAchieved = achieved;
            await _db.SaveChangesAsync();
            if (newlyAchieved)
                await _productivity.LogActivityAsync("Goal", goal.Id, "Completed", $"Goal '{goal.Goal}' achieved");
            TempData["Success"] = "Goal updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var goal = await _db.PersonalGoals.FindAsync(id);
            if (goal == null) return NotFound();
            string name = goal.Goal;
            _db.PersonalGoals.Remove(goal); // linked tasks stay, their goal link is cleared
            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("Goal", id, "Deleted", $"Goal '{name}' deleted");
            TempData["Success"] = $"Goal '{name}' deleted. Its tasks were kept.";
            return RedirectToAction("Index");
        }
    }
}
