using LifeOS.MVC.Models;
using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    /// <summary>
    /// Every time a user logs hours on a skill, one row is written here.
    /// This gives full history and enables weekly/monthly aggregation.
    /// </summary>
    public class TechSkillLog
    {
        public int Id { get; set; }

        public int TechSkillId { get; set; }
        public TechSkill TechSkill { get; set; } = null!;

        public DateTime LogDate { get; set; } = DateTime.Today;

        public double Hours { get; set; }

        [MaxLength(200)]
        public string? TopicCovered { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        public DateTime LoggedAt { get; set; } = DateTime.Now;
    }
}