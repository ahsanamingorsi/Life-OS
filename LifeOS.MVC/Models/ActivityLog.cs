using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } = ""; // Habit | PersonalTask

        public int ReferenceId { get; set; } // Id of the related record

        [MaxLength(50)]
        public string Action { get; set; } = ""; // Completed | Updated | Created | Deleted

        [MaxLength(300)]
        public string? Detail { get; set; } // Human-readable detail

        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}