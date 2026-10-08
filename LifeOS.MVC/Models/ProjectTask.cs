using LifeOS.MVC.Models;
using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class ProjectTask
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(10)]
        public string Priority { get; set; } = "B"; // A | B | C

        [MaxLength(20)]
        public string Status { get; set; } = "Todo"; // Todo | InProgress | Done

        public DateTime? DueDate { get; set; }

        public int? EstimatedMinutes { get; set; }

        public int? ActualMinutes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(300)]
        public string? Notes { get; set; }
    }
}