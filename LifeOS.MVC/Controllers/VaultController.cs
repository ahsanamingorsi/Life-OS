
using LifeOS.Models;
using LifeOS.MVC.Data;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Controllers
{
    public class VaultController : Controller
    {
        private readonly AppDbContext _db;
        public VaultController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index(string? search, string? category)
        {
            var q = _db.VaultEntries.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                q = q.Where(v =>
                    v.Name.Contains(search) ||
                    (v.Username != null && v.Username.Contains(search)) ||
                    (v.Email != null && v.Email.Contains(search)) ||
                    (v.Url != null && v.Url.Contains(search)));

            if (!string.IsNullOrEmpty(category))
                q = q.Where(v => v.Category == category);

            var entries = await q
                .OrderByDescending(v => v.IsFavorite)
                .ThenBy(v => v.Category)
                .ThenBy(v => v.Name)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Category = category;
            return View(entries);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Name, string? Username,
            string? Email, string? PasswordPlain, string? Url, string? Notes,
            string? RecoveryEmail, string? PhoneNumber, string Category,
            bool TwoFactorEnabled, string? TwoFactorMethod, bool IsFavorite)
        {
            var entry = new VaultEntry
            {
                Name = Name,
                Username = Username,
                Email = Email,
                Url = Url,
                Notes = Notes,
                RecoveryEmail = RecoveryEmail,
                PhoneNumber = PhoneNumber,
                Category = Category,
                TwoFactorEnabled = TwoFactorEnabled,
                TwoFactorMethod = TwoFactorMethod,
                IsFavorite = IsFavorite,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            if (VaultCrypto.IsTooLong(PasswordPlain)) { TempData["Error"] = "Passwords can be up to 100 characters."; return RedirectToAction("Index"); }
            entry.PasswordEncoded = VaultCrypto.Encrypt(PasswordPlain);
            _db.VaultEntries.Add(entry);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"'{Name}' saved to vault.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string Name,
            string? Username, string? Email, string? PasswordPlain,
            string? Url, string? Notes, string? RecoveryEmail,
            string? PhoneNumber, string Category, bool TwoFactorEnabled,
            string? TwoFactorMethod, bool IsFavorite)
        {
            var entry = await _db.VaultEntries.FindAsync(Id);
            if (entry == null) return NotFound();

            entry.Name = Name;
            entry.Username = Username;
            entry.Email = Email;
            entry.Url = Url;
            entry.Notes = Notes;
            entry.RecoveryEmail = RecoveryEmail;
            entry.PhoneNumber = PhoneNumber;
            entry.Category = Category;
            entry.TwoFactorEnabled = TwoFactorEnabled;
            entry.TwoFactorMethod = TwoFactorMethod;
            entry.IsFavorite = IsFavorite;
            entry.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrEmpty(PasswordPlain))
            {
                if (VaultCrypto.IsTooLong(PasswordPlain)) { TempData["Error"] = "Passwords can be up to 100 characters."; return RedirectToAction("Index"); }
                entry.PasswordEncoded = VaultCrypto.Encrypt(PasswordPlain);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Updated.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Returns one password for the Show / Copy buttons. POST + antiforgery token,
        /// never cached, so the password is not in the page source.
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Reveal(int id)
        {
            var stored = await _db.VaultEntries.Where(v => v.Id == id).Select(v => v.PasswordEncoded).FirstOrDefaultAsync();
            if (stored == null) return NotFound();
            var plain = VaultCrypto.Decrypt(stored);
            if (plain == null) return StatusCode(409, new { error = "This password cannot be read by the current Windows user." });
            return Json(new { password = plain });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _db.VaultEntries.FindAsync(id);
            if (e != null) { _db.VaultEntries.Remove(e); await _db.SaveChangesAsync(); }
            TempData["Success"] = "Deleted from vault.";
            return RedirectToAction("Index");
        }
    }
}