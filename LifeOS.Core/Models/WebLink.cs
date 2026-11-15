using System.ComponentModel.DataAnnotations;

namespace LifeOS.Models
{
    public class WebLink
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = "";

        [Required, MaxLength(500)]
        public string Url { get; set; } = "";

        [MaxLength(400)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string Category { get; set; } = "General";
        // Tools | Learning | News | Social | Dev | Design | Finance | Entertainment | General

        [MaxLength(200)]
        public string? Tags { get; set; }

        public bool IsFavorite { get; set; } = false;

        public bool IsPinned { get; set; } = false;

        public int VisitCount { get; set; } = 0;

        public DateTime? LastVisited { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Domain
        {
            get
            {
                try
                {
                    return new Uri(Url).Host.Replace("www.", "");
                }
                catch { return Url; }
            }
        }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<string> TagList =>
            string.IsNullOrEmpty(Tags)
                ? new List<string>()
                : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(t => t.Trim())
                      .ToList();
    }
}