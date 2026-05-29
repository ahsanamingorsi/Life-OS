using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class HealthEntry
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        public bool WorkoutDone { get; set; }

        [MaxLength(200)]
        public string? WorkoutNote { get; set; }

        public double WaterLiters { get; set; }

        public double SleepHours { get; set; }

        [MaxLength(200)]
        public string? DietNote { get; set; }

        public double? WeightKg { get; set; }

        public int EnergyLevel { get; set; } // 1–10

        public bool DietClean { get; set; }
    }
}