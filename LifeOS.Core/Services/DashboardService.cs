using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Services
{
    /// <summary>Builds everything the dashboard shows from real data.</summary>
    public class DashboardService
    {
        private readonly AppDbContext _db;
        private readonly ProductivityService _productivity;
        private readonly TaskService _tasks;

        public DashboardService(AppDbContext db, ProductivityService productivity, TaskService tasks)
        {
            _db = db;
            _productivity = productivity;
            _tasks = tasks;
        }

        public async Task<DashboardViewModel> BuildAsync(DateTime now)
        {
            var today = now.Date;
            var tomorrow = today.AddDays(1);
            var weekStart = ProductivityService.WeekStart(today);
            var vm = new DashboardViewModel
            {
                Today = today,
                Greeting = now.Hour < 12 ? "Good morning" : now.Hour < 17 ? "Good afternoon" : "Good evening",
            };

            // ── Score: today, yesterday and the last 7 days ─────────────
            var todayScore = await _productivity.GetDailyScoreAsync(today);
            vm.HasScore = todayScore.HasData;
            vm.Score = (int)Math.Round(todayScore.ProductivityScore);
            vm.ScoreParts = todayScore.Areas;
            vm.ScoreLabel = !vm.HasScore ? "No data yet"
                : vm.Score >= 75 ? "Great day"
                : vm.Score >= 50 ? "Good day"
                : vm.Score >= 25 ? "Keep going"
                : "Just starting";
            vm.RecentActivity = todayScore.RecentActivity.Take(5).ToList();

            for (int i = 6; i >= 0; i--)
            {
                var d = today.AddDays(-i);
                var s = i == 0 ? todayScore : await _productivity.GetDailyScoreAsync(d);
                vm.Week.Add(new DayScore { Date = d, Score = s.HasData ? (int)Math.Round(s.ProductivityScore) : null });
            }
            var yesterday = vm.Week[^2].Score;
            if (vm.HasScore && yesterday.HasValue) vm.ScoreChange = vm.Score - yesterday.Value;

            // ── Today's tasks: due today, repeated today, plus open overdue ones ──
            var dayTasks = await _tasks.GetForDayAsync(today, includeOverdue: true);
            vm.TasksDueToday = todayScore.TotalPersonalTasks;
            vm.TasksDoneToday = todayScore.CompletedPersonalTasks;
            vm.Tasks = dayTasks
                .Take(10)
                .Select(d => new DashboardTask
                {
                    Id = d.Task.Id,
                    Title = d.Task.Title,
                    IsCompleted = d.IsDone,
                    IsOverdue = d.IsOverdue,
                    IsRepeated = d.Task.IsRepeated,
                    Repeat = d.Task.Repeat,
                    Priority = d.Task.Priority,
                    Meta = string.Join(" · ", new[]
                    {
                        d.IsOverdue ? "Overdue · " + d.Task.DueDate!.Value.ToString("dd MMM")
                            : d.Task.IsRepeated ? TaskService.RepeatText(d.Task) : "Due today",
                        d.Task.IsRepeated && d.Streak > 0 ? $"{d.Streak} streak" : null,
                        d.Task.Goal != null ? "Goal: " + d.Task.Goal.Goal : null,
                    }.Where(s => !string.IsNullOrEmpty(s))),
                })
                .ToList();
            int overdueCount = await _db.PersonalTasks.CountAsync(t => t.Repeat == "None" && !t.IsCompleted && t.DueDate < today);

            // ── Spiritual today + streak of full days ───────────────────
            vm.PrayersDone = todayScore.PrayersCompletedToday;
            vm.PrayersTotal = todayScore.TotalPrayersToday;
            var todayPrayers = await _db.SpiritualEntries.AsNoTracking().FirstOrDefaultAsync(s => s.Date >= today && s.Date < tomorrow);
            if (todayPrayers != null) vm.PrayerFlags = todayPrayers.Prayers;
            var fullDays = (await _db.SpiritualEntries.AsNoTracking()
                    .Where(s => s.Date >= today.AddDays(-400) && s.PrayersTotal > 0 && s.PrayersDone >= s.PrayersTotal)
                    .Select(s => s.Date)
                    .ToListAsync())
                .Select(d => d.Date).ToHashSet();
            var cursor = fullDays.Contains(today) ? today : today.AddDays(-1);
            while (fullDays.Contains(cursor)) { vm.PrayerStreak++; cursor = cursor.AddDays(-1); }

            // ── Week levels ─────────────────────────────────────────────
            vm.Levels.Add(new AreaLevel
            {
                Name = "Tasks", Icon = "tasks", Url = "/PersonalTasks",
                Percent = vm.TasksDueToday > 0 ? ProductivityService.Pct(vm.TasksDoneToday, vm.TasksDueToday) : null,
                Detail = vm.TasksDueToday > 0 ? $"{vm.TasksDoneToday} of {vm.TasksDueToday} done today" : "No tasks due today",
            });

            var activeProjects = await _db.Projects.AsNoTracking().Where(p => p.Status == "Active").ToListAsync();
            int projectTasksThisWeek = await _db.ProjectTasks.CountAsync(t => t.CompletedAt >= weekStart && t.CompletedAt < tomorrow);
            vm.Levels.Add(new AreaLevel
            {
                Name = "Projects", Icon = "projects", Url = "/Projects",
                Percent = activeProjects.Any() ? (int)Math.Round(activeProjects.Average(p => Math.Clamp(p.ProgressPercent, 0, 100))) : null,
                Detail = activeProjects.Any()
                    ? $"{activeProjects.Count} active · {projectTasksThisWeek} tasks done this week"
                    : "No active projects",
            });

            double studyTarget = await _db.StudySubjects.Where(s => s.Status == "Active").SumAsync(s => s.WeeklyTargetHours);
            double studyHours = await _db.StudySessions.Where(s => s.SessionDate >= weekStart && s.SessionDate < tomorrow).SumAsync(s => s.DurationHours);
            vm.Levels.Add(new AreaLevel
            {
                Name = "Studies", Icon = "studies", Url = "/Study",
                Percent = studyTarget > 0 ? ProductivityService.Pct(studyHours, studyTarget) : null,
                Detail = studyTarget > 0
                    ? $"{studyHours:0.#}h of {studyTarget:0.#}h weekly target"
                    : $"{studyHours:0.#}h this week · set a weekly target",
            });

            double skillTarget = await _db.TechSkills
                .Where(s => s.IsCurrent || s.SkillLogs.Any(l => l.LogDate >= weekStart && l.LogDate < weekStart.AddDays(7)))
                .SumAsync(s => s.WeeklyTargetHours);
            double skillHours = await _db.TechSkillLogs.Where(l => l.LogDate >= weekStart && l.LogDate < tomorrow).SumAsync(l => l.Hours);
            vm.Levels.Add(new AreaLevel
            {
                Name = "Skills", Icon = "skills", Url = "/TechGrowth",
                Percent = skillTarget > 0 ? ProductivityService.Pct(skillHours, skillTarget) : null,
                Detail = skillTarget > 0
                    ? $"{skillHours:0.#}h of {skillTarget:0.#}h weekly target"
                    : $"{skillHours:0.#}h this week · pick a current skill",
            });

            vm.Levels.Add(new AreaLevel
            {
                Name = "Spiritual", Icon = "spiritual", Url = "/Spiritual",
                Percent = ProductivityService.Pct(vm.PrayersDone, vm.PrayersTotal),
                Detail = $"{vm.PrayersDone} of {vm.PrayersTotal} prayers today",
            });

            var goals = await _db.PersonalGoals.AsNoTracking()
                .Include(g => g.Tasks).ThenInclude(t => t.Completions)
                .ToListAsync();
            var openGoals = goals.Where(g => !g.IsAchieved).ToList();
            int goalTasksThisWeek = goals.SelectMany(g => g.Tasks).Sum(t => t.IsRepeated
                ? t.Completions.Count(c => c.Date >= weekStart && c.Date < tomorrow)
                : (t.IsCompleted && t.CompletedAt >= weekStart && t.CompletedAt < tomorrow ? 1 : 0));
            vm.Levels.Add(new AreaLevel
            {
                Name = "Goals", Icon = "goals", Url = "/PersonalUpgrade",
                Percent = openGoals.Any() ? (int)Math.Round(openGoals.Average(TaskService.GoalProgress)) : null,
                Detail = goals.Any()
                    ? $"{goalTasksThisWeek} goal tasks done this week · {goals.Count(g => g.IsAchieved)} of {goals.Count} achieved"
                    : "No goals yet",
            });

            bool reviewThisWeek = await _db.ReviewEntries.AnyAsync(r => r.CreatedAt >= weekStart && r.CreatedAt < tomorrow);
            vm.Levels.Add(new AreaLevel
            {
                Name = "Review", Icon = "review", Url = "/Review",
                Percent = reviewThisWeek ? 100 : 0,
                Detail = reviewThisWeek ? "This week: written" : "This week: not written yet",
            });

            // ── Projects in focus ───────────────────────────────────────
            vm.FocusProjects = activeProjects
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.Deadline ?? DateTime.MaxValue)
                .ThenBy(p => p.Id)
                .Take(3)
                .ToList();

            // ── Needs attention ─────────────────────────────────────────
            if (overdueCount > 0)
                vm.Alerts.Add(new DashboardAlert { Text = overdueCount == 1 ? "1 task overdue" : $"{overdueCount} tasks overdue", Url = "/PersonalTasks" });
            if (!reviewThisWeek && today.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday or DayOfWeek.Sunday)
                vm.Alerts.Add(new DashboardAlert { Text = "Weekly review not written", Url = "/Review" });

            // ── Library ─────────────────────────────────────────────────
            vm.LinkCount = await _db.WebLinks.CountAsync();
            vm.LinksPinned = await _db.WebLinks.CountAsync(l => l.IsPinned);

            return vm;
        }
    }
}
