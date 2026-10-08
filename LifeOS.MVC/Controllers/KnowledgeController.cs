
using LifeOS.Models;
using LifeOS.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Controllers
{
    public class KnowledgeController : Controller
    {
        private readonly AppDbContext _db;
        public KnowledgeController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index(
            string? search, string? category, string? type, string? tag)
        {
            var q = _db.KnowledgeEntries.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                q = q.Where(e =>
                    e.Title.Contains(search) ||
                    (e.Content != null && e.Content.Contains(search)) ||
                    (e.Tags != null && e.Tags.Contains(search)) ||
                    (e.Source != null && e.Source.Contains(search)));

            if (!string.IsNullOrEmpty(category))
                q = q.Where(e => e.Category == category);

            if (!string.IsNullOrEmpty(type))
                q = q.Where(e => e.Type == type);

            if (!string.IsNullOrEmpty(tag))
                q = q.Where(e => e.Tags != null && e.Tags.Contains(tag));

            var entries = await q
                .OrderByDescending(e => e.IsPinned)
                .ThenByDescending(e => e.IsFavorite)
                .ThenByDescending(e => e.UpdatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Type = type;
            ViewBag.Tag = tag;

            // All tags for filter cloud
            var allTags = await _db.KnowledgeEntries
                .Where(e => e.Tags != null)
                .Select(e => e.Tags!)
                .ToListAsync();
            ViewBag.AllTags = allTags
                .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(t => t.Trim())
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .Take(20)
                .Select(g => g.Key)
                .ToList();

            return View(entries);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KnowledgeEntry entry)
        {
            entry.CreatedAt = DateTime.Now;
            entry.UpdatedAt = DateTime.Now;
            _db.KnowledgeEntries.Add(entry);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"'{entry.Title}' saved to Knowledge Base.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> View(int id)
        {
            var entry = await _db.KnowledgeEntries.FindAsync(id);
            if (entry == null) return NotFound();
            return View(entry);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var entry = await _db.KnowledgeEntries.FindAsync(id);
            if (entry == null) return NotFound();
            return View(entry);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KnowledgeEntry updated)
        {
            var entry = await _db.KnowledgeEntries.FindAsync(updated.Id);
            if (entry == null) return NotFound();

            entry.Title = updated.Title;
            entry.Content = updated.Content;
            entry.Category = updated.Category;
            entry.Type = updated.Type;
            entry.Tags = updated.Tags;
            entry.Source = updated.Source;
            entry.IsPinned = updated.IsPinned;
            entry.IsFavorite = updated.IsFavorite;
            entry.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(int id)
        {
            var e = await _db.KnowledgeEntries.FindAsync(id);
            if (e != null) { e.IsPinned = !e.IsPinned; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var e = await _db.KnowledgeEntries.FindAsync(id);
            if (e != null) { e.IsFavorite = !e.IsFavorite; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _db.KnowledgeEntries.FindAsync(id);
            if (e != null) { _db.KnowledgeEntries.Remove(e); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Deleted.";
            return RedirectToAction("Index");
        }
    }
}