using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class SpiritualController : Controller
    {
        private readonly AppDbContext _db;
        public SpiritualController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var todayEntry = await _db.SpiritualEntries.FirstOrDefaultAsync(s => s.Date == today);
            var streak = await _db.SpiritualEntries.CountAsync(s => s.PrayersDone == s.PrayersTotal);
            ViewBag.Streak = streak;
            return View(todayEntry ?? new SpiritualEntry());
        }

        [HttpPost]
        public async Task<IActionResult> Save(SpiritualEntry entry)
        {
            var existing = await _db.SpiritualEntries.FirstOrDefaultAsync(s => s.Date == entry.Date);
            if (existing == null) { _db.SpiritualEntries.Add(entry); }
            else
            {
                existing.PrayersDone = entry.PrayersDone;
                existing.PrayersTotal = entry.PrayersTotal;
                existing.GratitudeNote = entry.GratitudeNote;
                existing.ReflectionNote = entry.ReflectionNote;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}