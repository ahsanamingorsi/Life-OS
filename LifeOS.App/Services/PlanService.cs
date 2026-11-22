using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.App.Services
{
    /// <summary>Daily plan helpers: time text, and keeping a plan item and its linked task in step.</summary>
    public static class PlanService
    {
        public static string TimeText(int minutes)
        {
            var t = DateTime.Today.AddMinutes(minutes);
            return t.ToString("h:mm tt");
        }

        public static string RangeText(PlanItem p) =>
            p.StartMinutes == null ? "Anytime"
            : p.EndMinutes == null ? TimeText(p.StartMinutes.Value)
            : $"{TimeText(p.StartMinutes.Value)} – {TimeText(p.EndMinutes.Value)}";

        public static int NowMinutes() => (int)DateTime.Now.TimeOfDay.TotalMinutes;

        /// <summary>Flips a plan item and also completes / reopens the task it points to.</summary>
        public static async Task<bool?> ToggleAsync(IServiceProvider sp, int planItemId)
        {
            var db = sp.GetRequiredService<AppDbContext>();
            var item = await db.PlanItems.FindAsync(planItemId);
            if (item == null) return null;
            bool done = !item.IsDone;
            item.IsDone = done;
            await db.SaveChangesAsync();

            if (item.SourceId is int sid)
            {
                switch (item.SourceKind)
                {
                    case "Task":
                        var tasks = sp.GetRequiredService<TaskService>();
                        var t = await db.PersonalTasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sid);
                        if (t != null && !t.IsRepeated)
                        {
                            if (t.IsCompleted != done) await tasks.ToggleAsync(sid, item.Date);
                        }
                        else if (t != null)
                        {
                            bool doneOnDay = await db.TaskCompletions.AnyAsync(c => c.PersonalTaskId == sid && c.Date == item.Date);
                            if (doneOnDay != done) await tasks.ToggleAsync(sid, item.Date);
                        }
                        break;
                    case "ProjectTask":
                        var pt = await db.ProjectTasks.FindAsync(sid);
                        if (pt != null && pt.IsCompleted != done)
                        {
                            pt.IsCompleted = done;
                            pt.Status = done ? "Done" : "Todo";
                            pt.CompletedAt = done ? DateTime.Now : null;
                            await db.SaveChangesAsync();
                            await ProjectProgress.RecalculateAsync(db, pt.ProjectId);
                        }
                        break;
                    case "StudyTask":
                        var st = await db.StudyTasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sid);
                        if (st != null && st.IsCompleted != done) await StudyTaskItems.ToggleAsync(db, sid);
                        break;
                }
            }
            return done;
        }
    }
}
