
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

        /// <summary>
        /// Adds https:// when no scheme is given and accepts only http/https web addresses,
        /// so links like "javascript:" can never be saved. Returns null when the address is not valid.
        /// </summary>
        private static string? CleanUrl(string? url)
        {
            url = url?.Trim();
            if (string.IsNullOrEmpty(url)) return null;
            if (!url.Contains("://")) url = "https://" + url;
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                   && !string.IsNullOrEmpty(uri.Host)
                ? uri.AbsoluteUri
                : null;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WebLink link)
        {
            var url = CleanUrl(link.Url);
            if (url == null) { TempData["Error"] = "Enter a valid web address (http or https)."; return RedirectToAction("Index"); }
            link.Url = url;

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
            var url = CleanUrl(updated.Url);
            if (url == null) { TempData["Error"] = "Enter a valid web address (http or https)."; return RedirectToAction("Index"); }

            link.Name = updated.Name;
            link.Url = url;
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