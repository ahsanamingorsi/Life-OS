using LifeOS.MVC.Models;

namespace LifeOS.MVC.Models
{
    public class ProjectDetailViewModel
    {
        public Project Project { get; set; } = null!;

        public List<ProjectTask> Tasks { get; set; } = new();

        // Grouped by status for Kanban view
        public List<ProjectTask> TodoTasks
            => Tasks.Where(t => t.Status == "Todo").OrderBy(t => t.SortOrder).ThenBy(t => t.Priority).ToList();

        public List<ProjectTask> InProgressTasks
            => Tasks.Where(t => t.Status == "InProgress").OrderBy(t => t.SortOrder).ToList();

        public List<ProjectTask> DoneTasks
            => Tasks.Where(t => t.Status == "Done").OrderByDescending(t => t.CompletedAt).ToList();

        // Stats
        public int TotalTasks => Tasks.Count;
        public int CompletedTasks => Tasks.Count(t => t.IsCompleted);
        public int OverdueTasks => Tasks.Count(t =>
            !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Today);
        public int ProgressPercent => TotalTasks > 0
            ? (int)((double)CompletedTasks / TotalTasks * 100) : 0;

        // Time tracking
        public int TotalEstimatedMinutes => Tasks.Sum(t => t.EstimatedMinutes ?? 0);
        public int TotalActualMinutes => Tasks.Sum(t => t.ActualMinutes ?? 0);
    }
}