using LifeOS.MVC.Models;

namespace LifeOS.Models
{
    public class GlobalSearchViewModel
    {
        public string Query { get; set; } = "";

        public List<KnowledgeEntry> KnowledgeResults { get; set; } = new();
        public List<WebLink> LinkResults { get; set; } = new();
        public List<VaultEntry> VaultResults { get; set; } = new();
        public List<Project> ProjectResults { get; set; } = new();
        public List<PersonalTask> TaskResults { get; set; } = new();
        public List<Habit> HabitResults { get; set; } = new();
        public List<TechSkill> SkillResults { get; set; } = new();
        public List<StudySubject> StudyResults { get; set; } = new();

        public int TotalResults =>
            KnowledgeResults.Count + LinkResults.Count + VaultResults.Count +
            ProjectResults.Count + TaskResults.Count + HabitResults.Count +
            SkillResults.Count + StudyResults.Count;
    }
}