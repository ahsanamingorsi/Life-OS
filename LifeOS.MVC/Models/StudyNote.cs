using LifeOS.MVC.Models;
using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class StudyNote
    {
        public int Id { get; set; }

        public int StudySubjectId { get; set; }
        public StudySubject StudySubject { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [MaxLength(4000)]
        public string? Content { get; set; }

        [MaxLength(200)]
        public string? Tags { get; set; }

        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}