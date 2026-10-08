using LifeOS.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LifeOS.MVC.Controllers;
public class PlannerController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(DateTime? date)
    {
        var day = (date ?? DateTime.Today).Date;
        ViewBag.Tasks = await db.PersonalTasks.AsNoTracking().Where(t => t.DueDate != null && t.DueDate.Value.Date == day || !t.IsCompleted && (t.DueDate == null || t.DueDate < day)).OrderBy(t => t.IsCompleted).ThenBy(t => t.Priority).ThenBy(t => t.DueDate).ToListAsync();
        ViewBag.Projects = await db.Projects.AsNoTracking().Where(p => p.Status == "Active").OrderBy(p => p.Priority).ThenBy(p => p.Deadline).Take(3).ToListAsync();
        ViewBag.Habits = await db.Habits.AsNoTracking().Include(h => h.Streak).Where(h => h.IsActive).OrderBy(h => h.Name).ToListAsync();
        ViewBag.DoneToday = await db.HabitLogs.Where(l => l.CompletedDate.Date == day).Select(l => l.HabitId).ToListAsync();
        ViewBag.Today = day;
        return View();
    }
}
