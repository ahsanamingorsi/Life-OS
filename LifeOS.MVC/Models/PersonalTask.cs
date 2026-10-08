using System.ComponentModel.DataAnnotations;

namespace LifeOS.MVC.Models
{
    public class PersonalTask
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        [MaxLength(10), RegularExpression("A|B|C")]
        public string Priority { get; set; } = "B"; // A | B | C

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
