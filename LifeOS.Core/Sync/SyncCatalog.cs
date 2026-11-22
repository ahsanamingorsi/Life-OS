using LifeOS.Models;
using LifeOS.MVC.Models;

namespace LifeOS.MVC.Sync
{
    /// <summary>A record that was deleted, so the other device can delete it too.</summary>
    public class SyncTombstone
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = "";
        public Guid SyncId { get; set; }
        public long DeletedTicks { get; set; }
    }

    /// <summary>Which tables are synced between devices, and in which order (parents first).</summary>
    public static class SyncCatalog
    {
        public const string SyncIdProp = "SyncId";
        public const string UpdatedProp = "UpdatedTicks";

        public static readonly Type[] Ordered =
        {
            typeof(PersonalGoal), typeof(TechSkill), typeof(Project), typeof(StudySubject), typeof(Habit),
            typeof(PersonalTask), typeof(TaskCompletion), typeof(ProjectTask), typeof(StudyTask), typeof(StudySession),
            typeof(StudyNote), typeof(TechSkillLog), typeof(HabitLog), typeof(SpiritualEntry), typeof(ReviewEntry),
            typeof(WebLink), typeof(DayReflection), typeof(PlanItem), typeof(FocusSession),
        };

        private static readonly HashSet<Type> Set = new(Ordered);
        public static bool IsSynced(Type t) => Set.Contains(t);

        /// <summary>Records that are "the same" when these values match, even if made separately on two devices.</summary>
        public static readonly Dictionary<Type, string[]> NaturalKeys = new()
        {
            [typeof(SpiritualEntry)] = new[] { "Date" },
            [typeof(DayReflection)] = new[] { "Date" },
            [typeof(TaskCompletion)] = new[] { "PersonalTaskId", "Date" },
            [typeof(HabitLog)] = new[] { "HabitId", "Date" },
        };

        /// <summary>Rows that point at another table by (kind, id): (kind property, id property, kind to table).</summary>
        public static readonly Dictionary<Type, (string KindProp, string IdProp)> Polymorphic = new()
        {
            [typeof(PlanItem)] = ("SourceKind", "SourceId"),
            [typeof(FocusSession)] = ("Kind", "RefId"),
        };

        public static Type? TypeForKind(string? kind) => kind switch
        {
            "Task" => typeof(PersonalTask),
            "ProjectTask" => typeof(ProjectTask),
            "StudyTask" => typeof(StudyTask),
            "StudySubject" => typeof(StudySubject),
            "Skill" => typeof(TechSkill),
            _ => null,
        };
    }
}
