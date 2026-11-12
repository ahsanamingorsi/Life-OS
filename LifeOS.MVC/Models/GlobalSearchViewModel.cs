using LifeOS.MVC.Models;

namespace LifeOS.Models
{
    public class GlobalSearchViewModel
    {
        public string Query { get; set; } = "";

        public List<WebLink> LinkResults { get; set; } = new();
        public List<VaultEntry> VaultResults { get; set; } = new();
        public List<Project> ProjectResults { get; set; } = new();
        public List<PersonalTask> TaskResults { get; set; } = new();
        public List<TechSkill> SkillResults { get; set; } = new();
        public List<StudySubject> StudyResults { get; set; } = new();
        public List<ProjectTask> ProjectTaskResults { get; set; } = new();
        public List<StudyTask> StudyTaskResults { get; set; } = new();
        public List<PersonalGoal> GoalResults { get; set; } = new();
        public List<SpiritualEntry> SpiritualResults { get; set; } = new();

        public int TotalResults =>
            LinkResults.Count + VaultResults.Count +
            ProjectResults.Count + TaskResults.Count +
            SkillResults.Count + StudyResults.Count +
            ProjectTaskResults.Count + StudyTaskResults.Count + GoalResults.Count +
            SpiritualResults.Count;
    }
}
