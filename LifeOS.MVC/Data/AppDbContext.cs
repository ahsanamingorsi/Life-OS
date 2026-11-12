using LifeOS.Models;
using LifeOS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TechSkill> TechSkills { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<SpiritualEntry> SpiritualEntries { get; set; }
        public DbSet<PersonalGoal> PersonalGoals { get; set; }
        public DbSet<ReviewEntry> ReviewEntries { get; set; }

        // ── TASKS ────────────────────────────────────────────────────────
        public DbSet<PersonalTask> PersonalTasks { get; set; }
        public DbSet<TaskCompletion> TaskCompletions { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        // ── PROJECT TASKS (NEW) ───────────────────────────────────────
        public DbSet<ProjectTask> ProjectTasks { get; set; }

        // ── STUDY MODULE (NEW) ────────────────────────────────────────
        public DbSet<StudySubject> StudySubjects { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        public DbSet<StudyTask> StudyTasks { get; set; }
        public DbSet<StudyNote> StudyNotes { get; set; }
        // ── TECH SKILL LOGS (NEW) ─────────────────────────────────────────
        public DbSet<TechSkillLog> TechSkillLogs { get; set; }
        // ── LINKS, VAULT ─────────────────────────────────────────────────
        public DbSet<WebLink> WebLinks { get; set; }
        public DbSet<VaultEntry> VaultEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Spiritual: one entry per day ──────────────────────────────
            modelBuilder.Entity<SpiritualEntry>()
                .HasIndex(s => s.Date)
                .IsUnique();

            // ── Tasks: repeat completions and goal link ──────────────────
            modelBuilder.Entity<TaskCompletion>()
                .HasOne(c => c.PersonalTask)
                .WithMany(t => t.Completions)
                .HasForeignKey(c => c.PersonalTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // One completion per task per day
            modelBuilder.Entity<TaskCompletion>()
                .HasIndex(c => new { c.PersonalTaskId, c.Date })
                .IsUnique();

            // Deleting a goal keeps its tasks; they just lose the link
            modelBuilder.Entity<PersonalTask>()
                .HasOne(t => t.Goal)
                .WithMany(g => g.Tasks)
                .HasForeignKey(t => t.GoalId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── ProjectTask constraints ───────────────────────────────
            modelBuilder.Entity<ProjectTask>()
                .HasOne(t => t.Project)
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudyTask>().HasOne(t => t.StudySubject).WithMany(s => s.Tasks).HasForeignKey(t => t.StudySubjectId).OnDelete(DeleteBehavior.Cascade);
            // ── Study constraints ─────────────────────────────────────
            modelBuilder.Entity<StudySession>()
                .HasOne(s => s.StudySubject)
                .WithMany(sub => sub.StudySessions)
                .HasForeignKey(s => s.StudySubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudyNote>()
                .HasOne(n => n.StudySubject)
                .WithMany(sub => sub.StudyNotes)
                .HasForeignKey(n => n.StudySubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // TechSkillLog → TechSkill cascade
            modelBuilder.Entity<TechSkillLog>()
                .HasOne(l => l.TechSkill)
                .WithMany(s => s.SkillLogs)
                .HasForeignKey(l => l.TechSkillId)
                .OnDelete(DeleteBehavior.Cascade);

            

           
        }
        }
}
