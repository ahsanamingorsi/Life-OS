using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _db;
        public ProjectsController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var projects = await _db.Projects.OrderBy(p => p.Priority).ThenBy(p => p.Deadline).ToListAsync();
            return View(projects);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Project project)
        {
            if (ModelState.IsValid)
            {
                _db.Projects.Add(project);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, int progress)
        {
            var proj = await _db.Projects.FindAsync(id);
            if (proj != null)
            {
                proj.Status = status;
                proj.ProgressPercent = Math.Clamp(progress, 0, 100);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var proj = await _db.Projects.FindAsync(id);
            if (proj != null) { _db.Projects.Remove(proj); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}