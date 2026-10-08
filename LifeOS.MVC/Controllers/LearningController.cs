using LifeOS.Models;
using LifeOS.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeO.MVCS.Controllers
{
    /// <summary>
    /// Learning hub — merges TechSkills + StudySubjects into one unified learning module.
    /// </summary>
    public class LearningController : Controller
    {
        private readonly AppDbContext _db;
        public LearningController(AppDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var skills = await _db.TechSkills
                .Include(s => s.SkillLogs)
                .OrderByDescending(s => s.IsCurrent)
                .ThenByDescending(s => s.UpdatedAt)
                .ToListAsync();

            var subjects = await _db.StudySubjects
                .Include(s => s.StudySessions)
                .Include(s => s.StudyNotes)
                .Where(s => s.Status == "Active")
                .OrderBy(s => s.Name)
                .ToListAsync();

            var today = DateTime.Today;
            var weekStart = today.AddDays(-6);

            double skillHoursWeek = await _db.TechSkillLogs
                .Where(l => l.LogDate >= weekStart)
                .SumAsync(l => l.Hours);

            double studyHoursWeek = await _db.StudySessions
                .Where(s => s.SessionDate >= weekStart)
                .SumAsync(s => s.DurationHours);

            var recentSkillLogs = await _db.TechSkillLogs
                .Include(l => l.TechSkill)
                .Where(l => l.LogDate >= weekStart)
                .OrderByDescending(l => l.LoggedAt)
                .Take(8)
                .ToListAsync();

            var recentStudySessions = await _db.StudySessions
                .Include(s => s.StudySubject)
                .Where(s => s.SessionDate >= weekStart)
                .OrderByDescending(s => s.SessionDate)
                .Take(8)
                .ToListAsync();

            ViewBag.Skills = skills;
            ViewBag.Subjects = subjects;
            ViewBag.SkillHoursWeek = skillHoursWeek;
            ViewBag.StudyHoursWeek = studyHoursWeek;
            ViewBag.RecentSkillLogs = recentSkillLogs;
            ViewBag.RecentStudySessions = recentStudySessions;
            ViewBag.TotalSkillHours = skills.Sum(s => s.HoursLogged);
            ViewBag.TotalStudyHours = subjects.Sum(s => s.HoursLogged);

            return View();
        }
    }
}