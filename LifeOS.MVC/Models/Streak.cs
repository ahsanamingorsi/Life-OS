using LifeOS.MVC.Models;

namespace LifeOS.MVC.Models
{
    public class Streak
    {
        public int Id { get; set; }

        public int HabitId { get; set; }
        public Habit Habit { get; set; } = null!;

        public int CurrentStreak { get; set; } = 0;

        public int BestStreak { get; set; } = 0;

        public DateTime? LastCompletedDate { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}