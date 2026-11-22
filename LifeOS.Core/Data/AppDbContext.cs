using LifeOS.Models;
using LifeOS.MVC.Models;
using LifeOS.MVC.Sync;
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
        // ── LINKS ────────────────────────────────────────────────────────
        public DbSet<WebLink> WebLinks { get; set; }
        // ── PLANNER / HABITS / FOCUS ─────────────────────────────────────
        public DbSet<PlanItem> PlanItems { get; set; }
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitLog> HabitLogs { get; set; }
        public DbSet<FocusSession> FocusSessions { get; set; }
        public DbSet<DayReflection> DayReflections { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<SyncTombstone> SyncTombstones { get; set; }

        /// <summary>True while a sync is writing data from another device (keeps that device's timestamps).</summary>
        public bool SuppressSyncStamps { get; set; }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            StampSyncInfo();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            StampSyncInfo();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>Gives every new / changed synced record an id and a "last changed" time; remembers deletions.</summary>
        private void StampSyncInfo()
        {
            if (SuppressSyncStamps) return;
            long now = DateTime.UtcNow.Ticks;
            foreach (var entry in ChangeTracker.Entries().ToList())
            {
                var type = entry.Metadata.ClrType;
                if (!SyncCatalog.IsSynced(type)) continue;
                switch (entry.State)
                {
                    case EntityState.Added:
                        if ((Guid)entry.Property(SyncCatalog.SyncIdProp).CurrentValue! == Guid.Empty)
                            entry.Property(SyncCatalog.SyncIdProp).CurrentValue = Guid.NewGuid();
                        entry.Property(SyncCatalog.UpdatedProp).CurrentValue = now;
                        break;
                    case EntityState.Modified:
                        entry.Property(SyncCatalog.UpdatedProp).CurrentValue = now;
                        break;
                    case EntityState.Deleted:
                        var id = (Guid)entry.Property(SyncCatalog.SyncIdProp).CurrentValue!;
                        if (id != Guid.Empty)
                            SyncTombstones.Add(new SyncTombstone { EntityType = type.Name, SyncId = id, DeletedTicks = now });
                        break;
                }
            }
        }

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

            // ── Planner / habits / focus ──────────────────────────────────
            modelBuilder.Entity<PlanItem>().HasIndex(p => p.Date);

            modelBuilder.Entity<HabitLog>()
                .HasOne(l => l.Habit)
                .WithMany(h => h.Logs)
                .HasForeignKey(l => l.HabitId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<HabitLog>().HasIndex(l => new { l.HabitId, l.Date }).IsUnique();

            modelBuilder.Entity<FocusSession>().HasIndex(f => f.StartedAt);
            modelBuilder.Entity<DayReflection>().HasIndex(r => r.Date).IsUnique();

            // ── Sync: every synced table gets a global id and a "last changed" time (hidden columns) ──
            foreach (var type in SyncCatalog.Ordered)
            {
                var entity = modelBuilder.Entity(type);
                entity.Property<Guid>(SyncCatalog.SyncIdProp);
                entity.Property<long>(SyncCatalog.UpdatedProp);
                entity.HasIndex(SyncCatalog.SyncIdProp);
            }
            modelBuilder.Entity<SyncTombstone>().HasIndex(t => new { t.EntityType, t.SyncId });
        }
        }
}
