namespace LifeOS.MVC.Models
{
    public class RepeatedTaskStreak
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = "";
        public string Repeat { get; set; } = "";
        public int CurrentStreak { get; set; }
        public int Last30Days { get; set; }
        public int Total { get; set; }
    }
}
