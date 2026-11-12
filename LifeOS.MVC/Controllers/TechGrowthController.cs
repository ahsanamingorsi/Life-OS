using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class TechGrowthController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ProductivityService _productivity;

        public TechGrowthController(AppDbContext db, ProductivityService productivity)
        {
            _db = db;
            _productivity = productivity;
        }

        // ── INDEX ────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var weekStart = DateTime.Today.AddDays(-6);

            var skills = await _db.TechSkills
                .Include(s => s.SkillLogs)
                .OrderByDescending(s => s.IsCurrent)
                .ThenByDescending(s => s.IsNext)
                .ThenBy(s => s.Category)
                .ThenByDescending(s => s.ProgressPercent)
                .ToListAsync();

            // Compute weekly hours per skill
            var weeklyMap = new Dictionary<int, double>();
            foreach (var skill in skills)
            {
                double wh = skill.SkillLogs
                    .Where(l => l.LogDate >= weekStart)
                    .Sum(l => l.Hours);
                weeklyMap[skill.Id] = wh;
                skill.WeeklyHours = wh; // set NotMapped property for view
            }

            var recentLogs = await _db.TechSkillLogs
                .Include(l => l.TechSkill)
                .Where(l => l.LogDate >= weekStart)
                .OrderByDescending(l => l.LoggedAt)
                .Take(10)
                .ToListAsync();

            var vm = new TechGrowthViewModel
            {
                Skills = skills,
                RecentLogs = recentLogs,
                WeeklyHoursMap = weeklyMap,
            };

            return View(vm);
        }

        // ── CREATE SKILL ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Category,Level,ProgressPercent,WeeklyTargetHours,IsCurrent,IsNext,ResourceUrl,ResourceName,Notes,CurrentTopic")] TechSkill skill)
        {
            skill.CreatedAt = DateTime.Now;
            skill.UpdatedAt = DateTime.Now;

            // If set as current, unset all others
            if (skill.IsCurrent)
                await UnsetAllCurrent();

            _db.TechSkills.Add(skill);
            await _db.SaveChangesAsync();

            await _productivity.LogActivityAsync("TechSkill", skill.Id, "Created",
                $"Skill '{skill.Name}' added to roadmap");
            TempData["Success"] = $"'{skill.Name}' added.";
            return RedirectToAction("Index");
        }

        // ── EDIT GET ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var skill = await _db.TechSkills.FindAsync(id);
            if (skill == null) return NotFound();
            return View(skill);
        }

        // ── EDIT POST ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TechSkill updated)
        {
            var skill = await _db.TechSkills.FindAsync(updated.Id);
            if (skill == null) return NotFound();

            if (updated.IsCurrent && !skill.IsCurrent)
                await UnsetAllCurrent();

            skill.Name = updated.Name;
            skill.Category = updated.Category;
            skill.Level = updated.Level;
            skill.ProgressPercent = Math.Clamp(updated.ProgressPercent, 0, 100);
            skill.WeeklyTargetHours = updated.WeeklyTargetHours;
            skill.IsCurrent = updated.IsCurrent;
            skill.IsNext = updated.IsNext;
            skill.ResourceUrl = updated.ResourceUrl;
            skill.ResourceName = updated.ResourceName;
            skill.Notes = updated.Notes;
            skill.CurrentTopic = updated.CurrentTopic;
            skill.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("TechSkill", skill.Id, "Updated",
                $"Skill '{skill.Name}' updated");
            TempData["Success"] = $"'{skill.Name}' updated.";
            return RedirectToAction("Index");
        }

        // ── LOG HOURS ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogHours(int skillId, double hours,
            int progress, string? topic, string? notes, DateTime? logDate)
        {
            if (!double.IsFinite(hours) || hours < 0 || hours > 24 || progress < -1 || progress > 100 || (hours == 0 && progress < 0))
            {
                TempData["Error"] = "Enter hours or a progress update.";
                return RedirectToAction("Index");
            }

            if (logDate.HasValue && logDate.Value.Date > DateTime.Today)
            {
                TempData["Error"] = "You cannot log hours for a future date.";
                return RedirectToAction("Index");
            }

            var skill = await _db.TechSkills.FindAsync(skillId);
            if (skill == null) return NotFound();

            // Log the session
            if (hours > 0)
            {
                _db.TechSkillLogs.Add(new TechSkillLog
                {
                    TechSkillId = skillId,
                    LogDate = logDate?.Date ?? DateTime.Today,
                    Hours = hours,
                    TopicCovered = topic,
                    Notes = notes,
                    LoggedAt = DateTime.Now
                });
                skill.HoursLogged += hours;
                if (!string.IsNullOrEmpty(topic))
                    skill.CurrentTopic = topic;
            }

            // Update progress if provided
            if (progress >= 0 && progress <= 100)
                skill.ProgressPercent = progress;

            skill.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            if (hours > 0)
            {
                await _productivity.LogActivityAsync("TechSkill", skillId, "HoursLogged",
                    $"{hours}h logged on '{skill.Name}'" + (topic != null ? $" — {topic}" : ""));
                TempData["Success"] = $"{hours}h logged on '{skill.Name}'.";
            }
            else
            {
                await _productivity.LogActivityAsync("TechSkill", skillId, "Updated",
                    $"Progress of '{skill.Name}' set to {skill.ProgressPercent}%");
                TempData["Success"] = $"Progress of '{skill.Name}' updated to {skill.ProgressPercent}%.";
            }
            return RedirectToAction("Index");
        }

        // ── DELETE LOG ENTRY ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLog(int logId)
        {
            var log = await _db.TechSkillLogs
                .Include(l => l.TechSkill)
                .FirstOrDefaultAsync(l => l.Id == logId);

            if (log != null)
            {
                log.TechSkill.HoursLogged =
                    Math.Max(0, log.TechSkill.HoursLogged - log.Hours);
                _db.TechSkillLogs.Remove(log);
                await _db.SaveChangesAsync();
            }

            TempData["Info"] = "Log entry removed.";
            return RedirectToAction("Index");
        }

        // ── SET CURRENT ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCurrent(int id)
        {
            var skill = await _db.TechSkills.FindAsync(id);
            if (skill == null) return NotFound();
            await UnsetAllCurrent();
            if (skill != null)
            {
                skill.IsCurrent = true;
                skill.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"'{skill.Name}' set as current skill.";
            }
            return RedirectToAction("Index");
        }

        // ── TOGGLE NEXT ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleNext(int id)
        {
            var skill = await _db.TechSkills.FindAsync(id);
            if (skill != null)
            {
                skill.IsNext = !skill.IsNext;
                skill.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // ── DELETE SKILL ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var skill = await _db.TechSkills.FindAsync(id);
            if (skill == null) return NotFound();

            string name = skill.Name;
            _db.TechSkills.Remove(skill); // cascades to SkillLogs
            await _db.SaveChangesAsync();

            await _productivity.LogActivityAsync("TechSkill", id, "Deleted",
                $"Skill '{name}' removed");
            TempData["Success"] = $"'{name}' removed.";
            return RedirectToAction("Index");
        }

        // ── HELPER ───────────────────────────────────────────────────
        private async Task UnsetAllCurrent()
        {
            var current = await _db.TechSkills
                .Where(s => s.IsCurrent)
                .ToListAsync();
            foreach (var s in current) s.IsCurrent = false;
            await _db.SaveChangesAsync();
        }
    }
}


