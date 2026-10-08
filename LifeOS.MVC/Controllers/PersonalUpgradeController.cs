using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class PersonalUpgradeController : Controller
    {
        private readonly AppDbContext _db;
        public PersonalUpgradeController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var goals = await _db.PersonalGoals.OrderBy(g => g.Category).ToListAsync();
            return View(goals);
        }

        [HttpPost]
        public async Task<IActionResult> Add([Bind(Prefix = "")] PersonalGoal entry)
        {
            if (ModelState.IsValid) { _db.PersonalGoals.Add(entry); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int id, int progress, bool achieved)
        {
            var goal = await _db.PersonalGoals.FindAsync(id);
            if (goal != null)
            {
                goal.ProgressPercent = Math.Clamp(progress, 0, 100);
                goal.IsAchieved = achieved;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var goal = await _db.PersonalGoals.FindAsync(id);
            if (goal != null) { _db.PersonalGoals.Remove(goal); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}
