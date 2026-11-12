using LifeOS.MVC.Data;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers;

/// <summary>My day: tasks and repeated tasks for one chosen day, plus projects in focus.</summary>
public class PlannerController(AppDbContext db, TaskService tasks) : Controller
{
    public async Task<IActionResult> Index(DateTime? date)
    {
        var today = DateTime.Today;
        var day = (date ?? today).Date;
        if (day < today.AddYears(-5) || day > today.AddYears(5)) day = today;

        var dayTasks = await tasks.GetForDayAsync(day, includeOverdue: day == today);
        ViewBag.Today = today;
        ViewBag.Day = day;
        ViewBag.OneTime = dayTasks.Where(t => !t.Task.IsRepeated).ToList();
        ViewBag.Repeated = dayTasks.Where(t => t.Task.IsRepeated).ToList();
        ViewBag.Projects = await db.Projects.AsNoTracking()
            .Where(p => p.Status == "Active")
            .OrderBy(p => p.Priority).ThenBy(p => p.Deadline ?? DateTime.MaxValue)
            .Take(3).ToListAsync();
        return View();
    }
}
