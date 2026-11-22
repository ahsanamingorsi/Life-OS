using LifeOS.MVC.Models;

namespace LifeOS.MVC.Services
{
    /// <summary>Pure habit rules (which days a habit applies, completion, streaks).</summary>
    public static class HabitMath
    {
        /// <summary>Monday = bit 1 ... Sunday = bit 64 (same as repeated tasks).</summary>
        public static int DayBit(DayOfWeek day) => 1 << (((int)day + 6) % 7);

        public static bool IsActiveOn(Habit h, DateTime day) => (h.DaysMask & DayBit(day.DayOfWeek)) != 0;

        /// <summary>0..1 how much of the day's goal is done.</summary>
        public static double Fraction(Habit h, int value)
        {
            if (h.Kind == "Count") return h.Target <= 0 ? (value > 0 ? 1 : 0) : Math.Min(1.0, value / (double)h.Target);
            return value > 0 ? 1 : 0;
        }

        public static bool IsDone(Habit h, int value) => Fraction(h, value) >= 1;

        /// <summary>
        /// Current streak of days the habit was completed. Days the habit does not apply to are skipped,
        /// and today does not break the streak while it is still open.
        /// </summary>
        public static int Streak(Habit h, IReadOnlyDictionary<DateTime, int> values, DateTime today)
        {
            int streak = 0;
            var d = today.Date;
            var first = h.CreatedAt.Date.AddDays(-1);
            for (int guard = 0; guard < 3650 && d >= first; guard++, d = d.AddDays(-1))
            {
                if (!IsActiveOn(h, d)) continue;
                bool done = values.TryGetValue(d, out var v) && IsDone(h, v);
                if (done) streak++;
                else if (d == today.Date) continue;
                else break;
            }
            return streak;
        }

        public static int BestStreak(Habit h, IReadOnlyDictionary<DateTime, int> values)
        {
            if (values.Count == 0) return 0;
            int best = 0, run = 0;
            for (var d = values.Keys.Min(); d <= values.Keys.Max(); d = d.AddDays(1))
            {
                if (!IsActiveOn(h, d)) continue;
                if (values.TryGetValue(d, out var v) && IsDone(h, v)) { run++; best = Math.Max(best, run); }
                else run = 0;
            }
            return best;
        }
    }
}
