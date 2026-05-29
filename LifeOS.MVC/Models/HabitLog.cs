using LifeOS.MVC.Models;
using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class HabitLog
    {
        public int Id { get; set; }

        public int HabitId { get; set; }
        public Habit Habit { get; set; } = null!;

        public DateTime CompletedDate { get; set; } = DateTime.Today;

        [MaxLength(200)]
        public string? Note { get; set; }

        public DateTime LoggedAt { get; set; } = DateTime.Now;
    }
}