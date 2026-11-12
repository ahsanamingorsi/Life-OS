
using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Controllers
{
    public class StudyController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ProductivityService _productivity;

        public StudyController(AppDbContext db, ProductivityService productivity)
        {
            _db = db;
            _productivity = productivity;
        }

        /// <summary>
        /// Subject progress: average of (hours ÷ target hours) and (tasks done ÷ all tasks),
        /// using only the parts that exist. With neither, the manual progress stays.
        /// </summary>
        public static bool IsAutoProgress(StudySubject s, int taskCount) => s.TargetHours > 0 || taskCount > 0;

        private async Task RecalculateProgressAsync(int subjectId)
        {
            var subject = await _db.StudySubjects.FindAsync(subjectId);
            if (subject == null) return;
            var tasks = await _db.StudyTasks.Where(t => t.StudySubjectId == subjectId).Select(t => t.IsCompleted).ToListAsync();

            var parts = new List<double>();
            if (subject.TargetHours > 0) parts.Add(Math.Min(1.0, subject.HoursLogged / subject.TargetHours));
            if (tasks.Any()) parts.Add((double)tasks.Count(done => done) / tasks.Count);
            if (parts.Any()) subject.ProgressPercent = Math.Clamp((int)Math.Round(parts.Average() * 100), 0, 100);
            await _db.SaveChangesAsync();
        }

        // ── DASHBOARD ────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-6);

            var subjects = await _db.StudySubjects
                .Include(s => s.StudySessions)
                .Include(s => s.StudyNotes)
                .OrderBy(s => s.Status)
                .ThenBy(s => s.Name)
                .ToListAsync();

            var thisWeekSessions = await _db.StudySessions
                .Include(s => s.StudySubject)
                .Where(s => s.SessionDate >= weekStart)
                .OrderByDescending(s => s.SessionDate)
                .ToListAsync();

            var todaySessions = thisWeekSessions
                .Where(s => s.SessionDate.Date == today)
                .ToList();

            var recentNotes = await _db.StudyNotes
                .Include(n => n.StudySubject)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            var vm = new StudyDashboardViewModel
            {
                Subjects = subjects,
                ThisWeekSessions = thisWeekSessions,
                TodaySessions = todaySessions,
                RecentNotes = recentNotes,
            };

            return View(vm);
        }

        // ── SUBJECT DETAIL ───────────────────────────────────────────
        public async Task<IActionResult> Subject(int id)
        {
            var subject = await _db.StudySubjects
                .Include(s => s.StudySessions)
                .Include(s => s.StudyNotes)
                .Include(s => s.Tasks)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null) return NotFound();

            var sessions = subject.StudySessions
                .OrderByDescending(s => s.SessionDate)
                .ToList();

            var notes = subject.StudyNotes
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.CreatedAt)
                .ToList();

            // Last 7 days heatmap
            var today = DateTime.Today;
            var last7 = new Dictionary<DateTime, double>();
            for (int i = 6; i >= 0; i--)
            {
                var d = today.AddDays(-i);
                last7[d] = sessions
                    .Where(s => s.SessionDate.Date == d)
                    .Sum(s => s.DurationHours);
            }

            var vm = new StudySubjectDetailViewModel
            {
                Subject = subject,
                Sessions = sessions,
                Notes = notes,
                Last7Days = last7
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddTask([Bind("StudySubjectId,Title,Notes,DueDate")] StudyTask task)
        {
            if (!await _db.StudySubjects.AnyAsync(s => s.Id == task.StudySubjectId)) return NotFound();
            task.Title = task.Title.Trim();
            if (string.IsNullOrWhiteSpace(task.Title)) { TempData["Error"] = "Enter a task name."; return RedirectToAction("Subject", new { id = task.StudySubjectId }); }
            task.Id = 0;
            task.CreatedAt = DateTime.Now;
            _db.StudyTasks.Add(task);
            await _db.SaveChangesAsync();
            await RecalculateProgressAsync(task.StudySubjectId);
            TempData["Success"] = "Subject task added.";
            return RedirectToAction("Subject", new { id = task.StudySubjectId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTask([Bind("Id,Title,Notes,DueDate")] StudyTask updated)
        {
            var task = await _db.StudyTasks.FindAsync(updated.Id);
            if (task == null) return NotFound();
            if (string.IsNullOrWhiteSpace(updated.Title)) { TempData["Error"] = "Enter a task name."; return RedirectToAction("Subject", new { id = task.StudySubjectId }); }
            task.Title = updated.Title.Trim();
            task.Notes = updated.Notes;
            task.DueDate = updated.DueDate;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Task updated.";
            return RedirectToAction("Subject", new { id = task.StudySubjectId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleTask(int id)
        {
            var task = await _db.StudyTasks.FindAsync(id);
            if (task == null) return NotFound();
            task.IsCompleted = !task.IsCompleted;
            task.CompletedAt = task.IsCompleted ? DateTime.Now : null;
            await _db.SaveChangesAsync();
            await RecalculateProgressAsync(task.StudySubjectId);
            return RedirectToAction("Subject", new { id = task.StudySubjectId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _db.StudyTasks.FindAsync(id);
            if (task == null) return NotFound();
            var subjectId = task.StudySubjectId;
            _db.StudyTasks.Remove(task);
            await _db.SaveChangesAsync();
            await RecalculateProgressAsync(subjectId);
            TempData["Success"] = "Task removed.";
            return RedirectToAction("Subject", new { id = subjectId });
        }
        // ── CREATE SUBJECT ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubject(StudySubject subject)
        {
            subject.CreatedAt = DateTime.Now;
            _db.StudySubjects.Add(subject);
            await _db.SaveChangesAsync();
            await _productivity.LogActivityAsync("Study", subject.Id, "Created",
                $"Subject '{subject.Name}' created");
            TempData["Success"] = $"'{subject.Name}' added.";
            return RedirectToAction("Subject", new { id = subject.Id });
        }

        // ── UPDATE SUBJECT ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSubject(StudySubject updated)
        {
            var subject = await _db.StudySubjects.FindAsync(updated.Id);
            if (subject == null) return NotFound();

            subject.Name = updated.Name;
            subject.Description = updated.Description;
            subject.Category = updated.Category;
            subject.Status = updated.Status;
            subject.TargetHours = updated.TargetHours;
            subject.WeeklyTargetHours = updated.WeeklyTargetHours;
            subject.ResourceUrl = updated.ResourceUrl;
            subject.CurrentTopic = updated.CurrentTopic;

            // Manual progress only when there is no target and no task to calculate it from.
            int taskCount = await _db.StudyTasks.CountAsync(t => t.StudySubjectId == subject.Id);
            if (!IsAutoProgress(subject, taskCount))
                subject.ProgressPercent = Math.Clamp(updated.ProgressPercent, 0, 100);

            await _db.SaveChangesAsync();
            await RecalculateProgressAsync(subject.Id);
            TempData["Success"] = $"'{subject.Name}' updated.";
            return RedirectToAction("Subject", new { id = subject.Id });
        }

        // ── DELETE SUBJECT ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _db.StudySubjects.FindAsync(id);
            if (subject == null) return NotFound();
            string name = subject.Name;
            _db.StudySubjects.Remove(subject);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"'{name}' deleted.";
            return RedirectToAction("Index");
        }

        // ── LOG SESSION ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogSession(StudySession session)
        {
            if (session.DurationHours <= 0 || session.DurationHours > 24 || !double.IsFinite(session.DurationHours)) { TempData["Error"] = "Enter study hours between 0 and 24."; return RedirectToAction("Subject", new { id = session.StudySubjectId }); }
            if (!await _db.StudySubjects.AnyAsync(s => s.Id == session.StudySubjectId)) return NotFound();
            session.LoggedAt = DateTime.Now;
            _db.StudySessions.Add(session);

            // Update subject's total hours
            var subject = await _db.StudySubjects.FindAsync(session.StudySubjectId);
            if (subject != null)
            {
                subject.HoursLogged += session.DurationHours;
                if (!string.IsNullOrEmpty(session.TopicCovered))
                    subject.CurrentTopic = session.TopicCovered;

            }

            await _db.SaveChangesAsync();
            await RecalculateProgressAsync(session.StudySubjectId);
            await _productivity.LogActivityAsync("Study", session.StudySubjectId,
                "SessionLogged",
                $"{session.DurationHours}h study session logged — {session.TopicCovered}");
            TempData["Success"] = $"{session.DurationHours}h session logged.";
            return RedirectToAction("Subject", new { id = session.StudySubjectId });
        }

        // ── DELETE SESSION ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(int sessionId, int subjectId)
        {
            var session = await _db.StudySessions.FindAsync(sessionId);
            if (session != null)
            {
                subjectId = session.StudySubjectId;
                var subject = await _db.StudySubjects.FindAsync(subjectId);
                if (subject != null)
                {
                    subject.HoursLogged = Math.Max(0,
                        subject.HoursLogged - session.DurationHours);
                }

                _db.StudySessions.Remove(session);
                await _db.SaveChangesAsync();
                await RecalculateProgressAsync(subjectId);
            }
            TempData["Info"] = "Session removed.";
            return RedirectToAction("Subject", new { id = subjectId });
        }

        // ── ADD NOTE ─────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(StudyNote note)
        {
            note.CreatedAt = DateTime.Now;
            note.UpdatedAt = DateTime.Now;
            _db.StudyNotes.Add(note);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Note '{note.Title}' saved.";
            return RedirectToAction("Subject", new { id = note.StudySubjectId });
        }

        // ── EDIT NOTE ────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNote(StudyNote updated)
        {
            var note = await _db.StudyNotes.FindAsync(updated.Id);
            if (note == null) return NotFound();

            note.Title = updated.Title;
            note.Content = updated.Content;
            note.Tags = updated.Tags;
            note.IsPinned = updated.IsPinned;
            note.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Note updated.";
            return RedirectToAction("Subject", new { id = note.StudySubjectId });
        }

        // ── DELETE NOTE ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNote(int noteId, int subjectId)
        {
            var note = await _db.StudyNotes.FindAsync(noteId);
            if (note != null)
            {
                subjectId = note.StudySubjectId;
                _db.StudyNotes.Remove(note);
                await _db.SaveChangesAsync();
            }
            TempData["Info"] = "Note deleted.";
            return RedirectToAction("Subject", new { id = subjectId });
        }

        // ── TOGGLE PIN NOTE ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(int noteId, int subjectId)
        {
            var note = await _db.StudyNotes.FindAsync(noteId);
            if (note != null)
            {
                subjectId = note.StudySubjectId;
                note.IsPinned = !note.IsPinned;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("Subject", new { id = subjectId });
        }
    }
}

