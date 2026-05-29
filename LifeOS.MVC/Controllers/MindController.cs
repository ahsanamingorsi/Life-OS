using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class MindController : Controller
    {
        private readonly AppDbContext _db;
        public MindController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var todayEntry = await _db.HabitEntries.FirstOrDefaultAsync(h => h.Date == today);
            var last7 = await _db.HabitEntries.Where(h => h.Date >= today.AddDays(-7)).OrderBy(h => h.Date).ToListAsync();
            ViewBag.Last7 = last7;
            return View(todayEntry ?? new HabitEntry());
        }

        [HttpPost]
        public async Task<IActionResult> Save(HabitEntry entry)
        {
            var existing = await _db.HabitEntries.FirstOrDefaultAsync(h => h.Date == entry.Date);
            if (existing == null) { _db.HabitEntries.Add(entry); }
            else
            {
                existing.PagesRead = entry.PagesRead;
                existing.ReadingNote = entry.ReadingNote;
                existing.CommunicationPractice = entry.CommunicationPractice;
                existing.CreativityTask = entry.CreativityTask;
                existing.LogicTask = entry.LogicTask;
                existing.ReflectionNote = entry.ReflectionNote;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}