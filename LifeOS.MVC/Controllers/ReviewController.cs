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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ReviewEntry entry)
        {
            if (ModelState.IsValid) { _db.ReviewEntries.Add(entry); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Period,StartDate,EndDate,Wins,MissedGoals,Notes,ImprovementSuggestions,OverallScore")] ReviewEntry updated)
        {
            var review = await _db.ReviewEntries.FindAsync(updated.Id);
            if (review == null) return NotFound();
            if (updated.Period is not ("Weekly" or "Monthly") || updated.StartDate == default || updated.EndDate < updated.StartDate || updated.OverallScore is < 1 or > 10)
            {
                TempData["Error"] = "Choose Weekly or Monthly, an end date on or after the start date, and a score from 1 to 10.";
                return RedirectToAction("Index");
            }

            review.Period = updated.Period;
            review.StartDate = updated.StartDate.Date;
            review.EndDate = updated.EndDate.Date;
            review.Wins = updated.Wins;
            review.MissedGoals = updated.MissedGoals;
            review.Notes = updated.Notes;
            review.ImprovementSuggestions = updated.ImprovementSuggestions;
            review.OverallScore = updated.OverallScore;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Review updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _db.ReviewEntries.FindAsync(id);
            if (review == null) return NotFound();
            _db.ReviewEntries.Remove(review);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Review deleted.";
            return RedirectToAction("Index");
        }
    }
}
