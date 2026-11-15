
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

            vm.LinkResults = await _db.WebLinks
                .Where(l => l.Name.Contains(term) ||
                            l.Url.Contains(term) ||
                            (l.Description != null && l.Description.Contains(term)) ||
                            (l.Tags != null && l.Tags.Contains(term)))
                .Take(8).ToListAsync();


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

            vm.ProjectTaskResults = await _db.ProjectTasks.Include(t => t.Project)
                .Where(t => t.Title.Contains(term) ||
                            (t.Description != null && t.Description.Contains(term)) ||
                            (t.Notes != null && t.Notes.Contains(term)))
                .OrderBy(t => t.IsCompleted).Take(5).ToListAsync();

            vm.StudyTaskResults = await _db.StudyTasks.Include(t => t.StudySubject)
                .Where(t => t.Title.Contains(term) || (t.Notes != null && t.Notes.Contains(term)))
                .OrderBy(t => t.IsCompleted).Take(5).ToListAsync();

            vm.GoalResults = await _db.PersonalGoals
                .Where(g => g.Goal.Contains(term) || (g.Notes != null && g.Notes.Contains(term)))
                .Take(5).ToListAsync();

            vm.SpiritualResults = await _db.SpiritualEntries
                .Where(s => (s.GratitudeNote != null && s.GratitudeNote.Contains(term)) ||
                            (s.ReflectionNote != null && s.ReflectionNote.Contains(term)))
                .OrderByDescending(s => s.Date).Take(5).ToListAsync();

            return View(vm);
        }
    }
}