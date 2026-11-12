using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Services
{
    /// <summary>One task as it appears on a given day.</summary>
    public class DayTask
    {
        public PersonalTask Task { get; set; } = null!;
        public DateTime Date { get; set; }
        public bool IsDone { get; set; }
        public bool IsOverdue { get; set; }   // one-time tasks only
        public int Streak { get; set; }       // repeated tasks only
    }

    /// <summary>Repeat rules, per-day completion and goal progress for personal tasks.</summary>
    public class TaskService
    {
        private readonly AppDbContext _db;

        public TaskService(AppDbContext db)
        {
            _db = db;
        }

        public static readonly (int Bit, string Short, string Long)[] WeekDays =
        {
            (1, "Mon", "Monday"), (2, "Tue", "Tuesday"), (4, "Wed", "Wednesday"), (8, "Thu", "Thursday"),
            (16, "Fri", "Friday"), (32, "Sat", "Saturday"), (64, "Sun", "Sunday"),
        };

        public static int DayBit(DayOfWeek day) => 1 << (((int)day + 6) % 7); // Monday = 1 … Sunday = 64

        public static DateTime StartDate(PersonalTask t) => (t.DueDate ?? t.CreatedAt).Date;

        /// <summary>Does this task appear on <paramref name="day"/>?</summary>
        public static bool OccursOn(PersonalTask t, DateTime day)
        {
            day = day.Date;
            if (!t.IsRepeated) return t.DueDate?.Date == day;

            var start = StartDate(t);
            if (day < start) return false;
            if (t.RepeatEndDate.HasValue && day > t.RepeatEndDate.Value.Date) return false;

            switch (t.Repeat)
            {
                case "Daily":
                    return true;
                case "Weekly":
                    int days = t.RepeatDays != 0 ? t.RepeatDays : DayBit(start.DayOfWeek);
                    return (days & DayBit(day.DayOfWeek)) != 0;
                case "Monthly":
                    int wanted = t.RepeatDayOfMonth ?? start.Day;
                    return day.Day == Math.Min(wanted, DateTime.DaysInMonth(day.Year, day.Month));
                default:
                    return false;
            }
        }

        /// <summary>Short human text for the repeat rule, e.g. "Every Mon, Thu".</summary>
        public static string RepeatText(PersonalTask t)
        {
            string text = t.Repeat switch
            {
                "Daily" => "Every day",
                "Weekly" => "Every " + string.Join(", ", WeekDays
                    .Where(w => ((t.RepeatDays != 0 ? t.RepeatDays : DayBit(StartDate(t).DayOfWeek)) & w.Bit) != 0)
                    .Select(w => w.Short)),
                "Monthly" => $"Every month on day {t.RepeatDayOfMonth ?? StartDate(t).Day}",
                _ => "",
            };
            if (t.IsRepeated && t.RepeatEndDate.HasValue) text += $" · until {t.RepeatEndDate.Value:dd MMM yyyy}";
            return text;
        }

        /// <summary>Consecutive completed occurrences, counted back from today.</summary>
        public static int Streak(PersonalTask t, ISet<DateTime> doneDates, DateTime today)
        {
            if (!t.IsRepeated) return 0;
            var start = StartDate(t);
            int streak = 0;
            bool first = true;
            for (var d = today.Date; d >= start && d > today.AddDays(-800); d = d.AddDays(-1))
            {
                if (!OccursOn(t, d)) continue;
                if (doneDates.Contains(d)) { streak++; first = false; continue; }
                if (first && d == today.Date) { first = false; continue; } // today not done yet: keep yesterday's streak
                break;
            }
            return streak;
        }

        /// <summary>
        /// Tasks for one day: one-time tasks due that day, repeated tasks that occur that day,
        /// and (optionally) open one-time tasks that were due before that day.
        /// </summary>
        public async Task<List<DayTask>> GetForDayAsync(DateTime day, bool includeOverdue)
        {
            day = day.Date;
            var next = day.AddDays(1);
            var today = DateTime.Today;

            var oneTime = await _db.PersonalTasks.AsNoTracking().Include(t => t.Goal)
                .Where(t => t.Repeat == "None" &&
                            ((t.DueDate >= day && t.DueDate < next) || (includeOverdue && !t.IsCompleted && t.DueDate < day)))
                .ToListAsync();

            var repeated = await _db.PersonalTasks.AsNoTracking().Include(t => t.Goal)
                .Include(t => t.Completions)
                .Where(t => t.Repeat != "None" && (t.DueDate == null || t.DueDate < next)
                            && (t.RepeatEndDate == null || t.RepeatEndDate >= day))
                .ToListAsync();

            var list = oneTime.Select(t => new DayTask
            {
                Task = t,
                Date = day,
                IsDone = t.IsCompleted,
                IsOverdue = !t.IsCompleted && t.DueDate < day,
            }).ToList();

            foreach (var t in repeated.Where(t => OccursOn(t, day)))
            {
                var done = t.Completions.Select(c => c.Date.Date).ToHashSet();
                list.Add(new DayTask
                {
                    Task = t,
                    Date = day,
                    IsDone = done.Contains(day),
                    Streak = Streak(t, done, day <= today ? day : today),
                });
            }

            return list
                .OrderBy(x => x.IsDone)
                .ThenByDescending(x => x.IsOverdue)
                .ThenBy(x => x.Task.Priority)
                .ThenBy(x => x.Task.Title)
                .ToList();
        }

        /// <summary>
        /// Marks a task done / not done. One-time tasks flip IsCompleted;
        /// repeated tasks add or remove that day's completion. Future days are refused.
        /// Returns the new state, or null when the task cannot be toggled for that day.
        /// </summary>
        public async Task<bool?> ToggleAsync(int taskId, DateTime? date)
        {
            var task = await _db.PersonalTasks.FindAsync(taskId);
            if (task == null) return null;

            if (!task.IsRepeated)
            {
                task.IsCompleted = !task.IsCompleted;
                task.CompletedAt = task.IsCompleted ? DateTime.Now : null;
                await _db.SaveChangesAsync();
                return task.IsCompleted;
            }

            var day = (date ?? DateTime.Today).Date;
            if (day > DateTime.Today || !OccursOn(task, day)) return null;

            var existing = await _db.TaskCompletions.FirstOrDefaultAsync(c => c.PersonalTaskId == taskId && c.Date == day);
            if (existing != null)
            {
                _db.TaskCompletions.Remove(existing);
                await _db.SaveChangesAsync();
                return false;
            }
            _db.TaskCompletions.Add(new TaskCompletion { PersonalTaskId = taskId, Date = day, CompletedAt = DateTime.Now });
            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Goal progress (0–100).
        /// • No linked tasks → the manual ProgressPercent.
        /// • One-time tasks part  = done ÷ total one-time tasks.
        /// • Repeated tasks part = completions ÷ TargetCount (only when a target is set).
        /// • Progress = average of the parts that exist, so a big repeat target does not
        ///   hide one-time work and the other way round.
        /// The goal must be loaded with Tasks and their Completions.
        /// </summary>
        public static int GoalProgress(PersonalGoal goal)
        {
            if (goal.IsAchieved) return 100;
            if (!goal.Tasks.Any()) return Math.Clamp(goal.ProgressPercent, 0, 100);

            var parts = new List<double>();
            var oneTime = goal.Tasks.Where(t => !t.IsRepeated).ToList();
            if (oneTime.Any()) parts.Add((double)oneTime.Count(t => t.IsCompleted) / oneTime.Count);

            int completions = goal.Tasks.Where(t => t.IsRepeated).Sum(t => t.Completions.Count);
            if (goal.TargetCount > 0 && goal.Tasks.Any(t => t.IsRepeated))
                parts.Add(Math.Min(1.0, (double)completions / goal.TargetCount.Value));

            if (!parts.Any()) return 0; // only repeated tasks and no target yet
            return (int)Math.Round(parts.Average() * 100);
        }

        /// <summary>Total times the goal's repeated tasks were completed.</summary>
        public static int GoalCompletions(PersonalGoal goal) =>
            goal.Tasks.Where(t => t.IsRepeated).Sum(t => t.Completions.Count);
    }
}
