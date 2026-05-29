using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class SpiritualEntry
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        public int PrayersDone { get; set; } // e.g. out of 5

        public int PrayersTotal { get; set; } = 5;

        public int DisciplineStreak { get; set; }

        [MaxLength(500)]
        public string? GratitudeNote { get; set; }

        [MaxLength(500)]
        public string? ReflectionNote { get; set; }
    }
}