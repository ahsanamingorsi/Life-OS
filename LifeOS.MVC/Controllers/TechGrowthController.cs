using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class TechGrowthController : Controller
    {
        private readonly AppDbContext _db;
        public TechGrowthController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var skills = await _db.TechSkills.OrderBy(s => s.Category).ToListAsync();
            return View(skills);
        }

        [HttpPost]
        public async Task<IActionResult> Add(TechSkill skill)
        {
            if (ModelState.IsValid)
            {
                _db.TechSkills.Add(skill);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int id, int progress, double hours)
        {
            var skill = await _db.TechSkills.FindAsync(id);
            if (skill != null)
            {
                skill.ProgressPercent = Math.Clamp(progress, 0, 100);
                skill.HoursLogged += hours;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var skill = await _db.TechSkills.FindAsync(id);
            if (skill != null) { _db.TechSkills.Remove(skill); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}