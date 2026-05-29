using LifeOS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.MVC.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TechSkill> TechSkills { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<HealthEntry> HealthEntries { get; set; }
        public DbSet<HabitEntry> HabitEntries { get; set; }
        public DbSet<SpiritualEntry> SpiritualEntries { get; set; }
        public DbSet<PersonalGoal> PersonalGoals { get; set; }
        public DbSet<ReviewEntry> ReviewEntries { get; set; }

        // ── NEW TABLES ───────────────────────────────────────────────────
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitLog> HabitLogs { get; set; }
        public DbSet<Streak> Streaks { get; set; }
        public DbSet<PersonalTask> PersonalTasks { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // HabitLog: one log per habit per date
            modelBuilder.Entity<HabitLog>()
                .HasIndex(h => new { h.HabitId, h.CompletedDate })
                .IsUnique();

            // Streak: one streak record per habit
            modelBuilder.Entity<Streak>()
                .HasIndex(s => s.HabitId)
                .IsUnique();

            // Habit → HabitLog cascade delete
            modelBuilder.Entity<HabitLog>()
                .HasOne(h => h.Habit)
                .WithMany(h => h.HabitLogs)
                .HasForeignKey(h => h.HabitId)
                .OnDelete(DeleteBehavior.Cascade);

            // Habit → Streak cascade delete
            modelBuilder.Entity<Streak>()
                .HasOne(s => s.Habit)
                .WithOne(h => h.Streak)
                .HasForeignKey<Streak>(s => s.HabitId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        }
}