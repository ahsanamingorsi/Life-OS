using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class HealthController : Controller
    {
        private readonly AppDbContext _db;
        public HealthController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var todayEntry = await _db.HealthEntries.FirstOrDefaultAsync(h => h.Date == today);
            var last7 = await _db.HealthEntries.Where(h => h.Date >= today.AddDays(-7)).OrderBy(h => h.Date).ToListAsync();
            ViewBag.TodayEntry = todayEntry;
            ViewBag.Last7 = last7;
            return View(todayEntry ?? new HealthEntry());
        }

        [HttpPost]
        public async Task<IActionResult> Save(HealthEntry entry)
        {
            var existing = await _db.HealthEntries.FirstOrDefaultAsync(h => h.Date == entry.Date);
            if (existing == null) { _db.HealthEntries.Add(entry); }
            else
            {
                existing.WorkoutDone = entry.WorkoutDone;
                existing.WorkoutNote = entry.WorkoutNote;
                existing.WaterLiters = entry.WaterLiters;
                existing.SleepHours = entry.SleepHours;
                existing.DietNote = entry.DietNote;
                existing.WeightKg = entry.WeightKg;
                existing.EnergyLevel = entry.EnergyLevel;
                existing.DietClean = entry.DietClean;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}