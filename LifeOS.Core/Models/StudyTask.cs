using System.ComponentModel.DataAnnotations;
namespace LifeOS.MVC.Models;
public class StudyTask
{
    public int Id { get; set; }
    public int StudySubjectId { get; set; }
    [Required, MaxLength(200)]
    public string Title { get; set; } = "";
    [MaxLength(1000)]
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
    public StudySubject? StudySubject { get; set; }
}
