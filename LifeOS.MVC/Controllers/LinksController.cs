
using LifeOS.Models;
using LifeOS.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Controllers
{
    public class LinksController : Controller
    {
        private readonly AppDbContext _db;
        public LinksController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index(string? search, string? category)
        {
            var q = _db.WebLinks.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                q = q.Where(l =>
                    l.Name.Contains(search) ||
                    l.Url.Contains(search) ||
                    (l.Description != null && l.Description.Contains(search)) ||
                    (l.Tags != null && l.Tags.Contains(search)));

            if (!string.IsNullOrEmpty(category))
                q = q.Where(l => l.Category == category);

            var links = await q
                .OrderByDescending(l => l.IsPinned)
                .ThenByDescending(l => l.IsFavorite)
                .ThenByDescending(l => l.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Category = category;
            return View(links);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebLink link)
        {
            // Auto-add https if missing
            if (!link.Url.StartsWith("http"))
                link.Url = "https://" + link.Url;

            link.CreatedAt = DateTime.Now;
            _db.WebLinks.Add(link);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"'{link.Name}' saved.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(WebLink updated)
        {
            var link = await _db.WebLinks.FindAsync(updated.Id);
            if (link == null) return NotFound();

            link.Name = updated.Name;
            link.Url = updated.Url;
            link.Description = updated.Description;
            link.Category = updated.Category;
            link.Tags = updated.Tags;
            link.IsFavorite = updated.IsFavorite;
            link.IsPinned = updated.IsPinned;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Visit(int id)
        {
            var link = await _db.WebLinks.FindAsync(id);
            if (link != null)
            {
                link.VisitCount++;
                link.LastVisited = DateTime.Now;
                await _db.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var l = await _db.WebLinks.FindAsync(id);
            if (l != null) { l.IsFavorite = !l.IsFavorite; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var l = await _db.WebLinks.FindAsync(id);
            if (l != null) { _db.WebLinks.Remove(l); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Deleted.";
            return RedirectToAction("Index");
        }
    }
}