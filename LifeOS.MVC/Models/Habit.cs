using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class Habit
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = "";

        [MaxLength(300)]
        public string? Description { get; set; }

        [MaxLength(10)]
        public string Frequency { get; set; } = "Daily"; // Daily | Weekly

        [MaxLength(50)]
        public string Category { get; set; } = "General";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<HabitLog> HabitLogs { get; set; } = new List<HabitLog>();
        public Streak? Streak { get; set; }
    }
}