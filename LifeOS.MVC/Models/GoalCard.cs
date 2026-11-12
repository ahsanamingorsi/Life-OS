namespace LifeOS.MVC.Models
{
    public class GoalCard
    {
        public PersonalGoal Goal { get; set; } = null!;
        public int Progress { get; set; }          // 0–100
        public bool IsAuto { get; set; }           // true = calculated from linked tasks
        public int OneTimeDone { get; set; }
        public int OneTimeTotal { get; set; }
        public int RepeatedCount { get; set; }
        public int Completions { get; set; }       // repeated task completions in total
        public List<GoalTaskLine> Tasks { get; set; } = new();
    }

    public class GoalTaskLine
    {
        public PersonalTask Task { get; set; } = null!;
        public string Text { get; set; } = "";
        public bool IsDone { get; set; }           // repeated: done today; one-time: completed
    }
}
