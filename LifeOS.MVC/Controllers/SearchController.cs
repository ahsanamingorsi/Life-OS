
using LifeOS.Models;
using LifeOS.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _db;
        public SearchController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index(string? q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return View(new GlobalSearchViewModel { Query = "" });

            string term = q.Trim();

            var vm = new GlobalSearchViewModel { Query = term };

            vm.KnowledgeResults = await _db.KnowledgeEntries
                .Where(e => e.Title.Contains(term) ||
                            (e.Content != null && e.Content.Contains(term)) ||
                            (e.Tags != null && e.Tags.Contains(term)))
                .Take(8).ToListAsync();

            vm.LinkResults = await _db.WebLinks
                .Where(l => l.Name.Contains(term) ||
                            l.Url.Contains(term) ||
                            (l.Description != null && l.Description.Contains(term)) ||
                            (l.Tags != null && l.Tags.Contains(term)))
                .Take(8).ToListAsync();

            vm.VaultResults = await _db.VaultEntries
                .Where(v => v.Name.Contains(term) ||
                            (v.Username != null && v.Username.Contains(term)) ||
                            (v.Email != null && v.Email.Contains(term)))
                .Take(6).ToListAsync();

            vm.ProjectResults = await _db.Projects
                .Where(p => p.Title.Contains(term) ||
                            (p.Notes != null && p.Notes.Contains(term)))
                .Take(5).ToListAsync();

            vm.TaskResults = await _db.PersonalTasks
                .Where(t => t.Title.Contains(term) ||
                            (t.Description != null && t.Description.Contains(term)))
                .Take(5).ToListAsync();

           

            vm.SkillResults = await _db.TechSkills
                .Where(s => s.Name.Contains(term) ||
                            (s.Notes != null && s.Notes.Contains(term)))
                .Take(5).ToListAsync();

            vm.StudyResults = await _db.StudySubjects
                .Where(s => s.Name.Contains(term) ||
                            (s.Description != null && s.Description.Contains(term)))
                .Take(5).ToListAsync();

            return View(vm);
        }
    }
}