using System.ComponentModel.DataAnnotations;

namespace LifeOS.Models
{
    public class KnowledgeEntry
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [MaxLength(10000)]
        public string? Content { get; set; }

        [MaxLength(50)]
        public string Category { get; set; } = "General";
        // Tech | Business | Health | Philosophy | Religion | Science | Finance | General

        [MaxLength(200)]
        public string? Tags { get; set; } // comma-separated

        [MaxLength(200)]
        public string? Source { get; set; } // book, website, video, person

        [MaxLength(20)]
        public string Type { get; set; } = "Note";
        // Note | Lesson | Quote | Summary | Idea | Framework | Checklist

        public bool IsPinned { get; set; } = false;

        public bool IsFavorite { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Computed
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<string> TagList =>
            string.IsNullOrEmpty(Tags)
                ? new List<string>()
                : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(t => t.Trim())
                      .ToList();
    }
}