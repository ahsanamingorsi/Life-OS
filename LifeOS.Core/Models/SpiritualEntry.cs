using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class SpiritualEntry
    {
        public static readonly string[] PrayerNames = { "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha" };

        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        // ── Each prayer on its own ───────────────────────────────────
        public bool Fajr { get; set; }
        public bool Dhuhr { get; set; }
        public bool Asr { get; set; }
        public bool Maghrib { get; set; }
        public bool Isha { get; set; }

        // Kept in step with the five flags above when saving (used by the dashboard and score).
        public int PrayersDone { get; set; }

        public int PrayersTotal { get; set; } = 5;

        public int DisciplineStreak { get; set; }

        [MaxLength(500)]
        public string? GratitudeNote { get; set; }

        [MaxLength(500)]
        public string? ReflectionNote { get; set; }

        public bool[] Prayers => new[] { Fajr, Dhuhr, Asr, Maghrib, Isha };
    }
}
