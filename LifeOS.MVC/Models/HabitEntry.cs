using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class HabitEntry
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;

        public int PagesRead { get; set; }

        [MaxLength(200)]
        public string? ReadingNote { get; set; }

        public bool CommunicationPractice { get; set; }

        public bool CreativityTask { get; set; }

        public bool LogicTask { get; set; }

        [MaxLength(500)]
        public string? ReflectionNote { get; set; }
    }
}