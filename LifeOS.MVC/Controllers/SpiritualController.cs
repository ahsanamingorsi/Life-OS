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

        /// <summary>Entry for one day (today by default), last 30 days of history and the current streak.</summary>
        public async Task<IActionResult> Index(DateTime? date)
        {
            var today = DateTime.Today;
            var day = (date ?? today).Date;
            if (day > today) { TempData["Error"] = "You cannot log a future day."; day = today; }

            var entry = await _db.SpiritualEntries.AsNoTracking().FirstOrDefaultAsync(s => s.Date == day);

            var history = await _db.SpiritualEntries.AsNoTracking()
                .Where(s => s.Date >= today.AddDays(-29) && s.Date <= today)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            ViewBag.Day = day;
            ViewBag.Today = today;
            ViewBag.History = history;
            ViewBag.Streak = await CurrentStreakAsync(today);
            ViewBag.PerfectDays = await _db.SpiritualEntries.CountAsync(s => s.PrayersTotal > 0 && s.PrayersDone >= s.PrayersTotal);
            return View(entry ?? new SpiritualEntry { Date = day });
        }

        /// <summary>Consecutive days (ending today, or yesterday if today is not complete yet) with all prayers done.</summary>
        private async Task<int> CurrentStreakAsync(DateTime today)
        {
            var fullDays = (await _db.SpiritualEntries.AsNoTracking()
                    .Where(s => s.Date >= today.AddDays(-800) && s.PrayersTotal > 0 && s.PrayersDone >= s.PrayersTotal)
                    .Select(s => s.Date)
                    .ToListAsync())
                .Select(d => d.Date).ToHashSet();
            int streak = 0;
            var cursor = fullDays.Contains(today) ? today : today.AddDays(-1);
            while (fullDays.Contains(cursor)) { streak++; cursor = cursor.AddDays(-1); }
            return streak;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(SpiritualEntry entry)
        {
            entry.Date = entry.Date.Date;
            if (entry.Date == default || entry.Date > DateTime.Today)
            {
                TempData["Error"] = "Choose today or a past day.";
                return RedirectToAction("Index");
            }
            if (entry.PrayersTotal < 1 || entry.PrayersTotal > 5 || entry.PrayersDone < 0 || entry.PrayersDone > entry.PrayersTotal)
            {
                TempData["Error"] = "Prayers completed must be between zero and your daily total (1–5).";
                return RedirectToAction("Index", new { date = entry.Date.ToString("yyyy-MM-dd") });
            }

            var existing = await _db.SpiritualEntries.FirstOrDefaultAsync(s => s.Date == entry.Date);
            if (existing == null)
            {
                entry.Id = 0;
                _db.SpiritualEntries.Add(entry);
            }
            else
            {
                existing.PrayersDone = entry.PrayersDone;
                existing.PrayersTotal = entry.PrayersTotal;
                existing.GratitudeNote = entry.GratitudeNote;
                existing.ReflectionNote = entry.ReflectionNote;
            }
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Saved {entry.Date:dddd, d MMM}.";
            return RedirectToAction("Index", new { date = entry.Date.ToString("yyyy-MM-dd") });
        }
    }
}
