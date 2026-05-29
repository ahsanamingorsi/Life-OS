using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class ReviewController : Controller
    {
        private readonly AppDbContext _db;
        public ReviewController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var reviews = await _db.ReviewEntries.OrderByDescending(r => r.CreatedAt).Take(10).ToListAsync();
            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> Add(ReviewEntry entry)
        {
            if (ModelState.IsValid) { _db.ReviewEntries.Add(entry); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}