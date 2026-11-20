using LifeOS.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.App.Services
{
    public static class ProjectProgress
    {
        /// <summary>Progress, status and "next task" of a project follow its tasks.</summary>
        public static async Task RecalculateAsync(AppDbContext db, int projectId)
        {
            var project = await db.Projects.FindAsync(projectId);
            if (project == null) return;
            var tasks = await db.ProjectTasks.Where(t => t.ProjectId == projectId).ToListAsync();

            if (tasks.Any())
            {
                int done = tasks.Count(t => t.IsCompleted);
                project.ProgressPercent = (int)((double)done / tasks.Count * 100);
                if (project.ProgressPercent == 100) project.Status = "Done";
                else if (project.Status == "Done") project.Status = "Active";
                project.NextTask = tasks.Where(t => !t.IsCompleted).OrderBy(t => t.Priority).ThenBy(t => t.DueDate).FirstOrDefault()?.Title;
            }
            else
            {
                project.ProgressPercent = 0;
                project.NextTask = null;
                if (project.Status == "Done") project.Status = "Active";
            }
            await db.SaveChangesAsync();
        }
    }
}
