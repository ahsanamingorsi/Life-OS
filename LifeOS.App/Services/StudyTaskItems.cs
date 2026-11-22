using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.App.Services
{
    /// <summary>Study subject tasks, so they can sit next to normal tasks on Tasks, My day and the Dashboard.</summary>
    public static class StudyTaskItems
    {
        /// <summary>Id used for a study task where a normal task id is expected: negative, so the two never clash.</summary>
        public static int ToMixedId(int studyTaskId) => -studyTaskId;

        public static async Task<List<StudyTask>> LoadAllAsync(AppDbContext db) =>
            await db.StudyTasks.AsNoTracking().Include(t => t.StudySubject).ToListAsync();

        /// <summary>Flips done / not done and keeps the subject's progress in step. Returns the new state.</summary>
        public static async Task<(bool Done, string Title)?> ToggleAsync(AppDbContext db, int studyTaskId)
        {
            var task = await db.StudyTasks.FindAsync(studyTaskId);
            if (task == null) return null;
            task.IsCompleted = !task.IsCompleted;
            task.CompletedAt = task.IsCompleted ? DateTime.Now : null;
            await db.SaveChangesAsync();
            await StudyProgress.RecalculateAsync(db, task.StudySubjectId);
            return (task.IsCompleted, task.Title);
        }

        public static async Task DeleteAsync(AppDbContext db, int studyTaskId)
        {
            var task = await db.StudyTasks.FindAsync(studyTaskId);
            if (task == null) return;
            int subjectId = task.StudySubjectId;
            db.StudyTasks.Remove(task);
            await db.SaveChangesAsync();
            await StudyProgress.RecalculateAsync(db, subjectId);
        }
    }
}
