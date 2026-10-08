using LifeOS.MVC.Models;

namespace LifeOS.MVC.Models
{
    public class TechGrowthViewModel
    {
        public List<TechSkill> Skills { get; set; } = new();

        // Grouped by category for roadmap view
        public Dictionary<string, List<TechSkill>> ByCategory
            => Skills
                .GroupBy(s => s.Category)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.IsCurrent)
                                                 .ThenByDescending(s => s.ProgressPercent)
                                                 .ToList());

        // Key skills
        public TechSkill? CurrentSkill => Skills.FirstOrDefault(s => s.IsCurrent);
        public List<TechSkill> NextSkills => Skills.Where(s => s.IsNext && !s.IsCurrent).ToList();

        // Aggregates
        public double TotalHours => Skills.Sum(s => s.HoursLogged);
        public double WeekHours => Skills.Sum(s => s.WeeklyHours);
        public int AvgProgress => Skills.Any() ? (int)Skills.Average(s => s.ProgressPercent) : 0;
        public int TotalSkills => Skills.Count;
        public int MasteredSkills => Skills.Count(s => s.ProgressPercent >= 80);

        // Last 7 days log entries across all skills
        public List<TechSkillLog> RecentLogs { get; set; } = new();

        // Weekly hours per skill (for the week summary bar)
        // Key = SkillId, Value = hours this week
        public Dictionary<int, double> WeeklyHoursMap { get; set; } = new();
    }

    // Extend TechSkill with computed weekly hours (via partial-style extension)
    public static class TechSkillExtensions
    {
        public static double WeeklyHours(this TechSkill skill, Dictionary<int, double> map)
            => map.TryGetValue(skill.Id, out double h) ? h : 0;
    }
}

// Add computed property directly on the partial — add this to TechSkill.cs instead:
// For the view model we use a separate property bag approach via ViewBag