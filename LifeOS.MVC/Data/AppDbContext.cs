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

        // ── PROJECT TASKS (NEW) ───────────────────────────────────────
        public DbSet<ProjectTask> ProjectTasks { get; set; }

        // ── STUDY MODULE (NEW) ────────────────────────────────────────
        public DbSet<StudySubject> StudySubjects { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        public DbSet<StudyTask> StudyTasks { get; set; }
        public DbSet<StudyNote> StudyNotes { get; set; }
        // ── TECH SKILL LOGS (NEW) ─────────────────────────────────────────
        public DbSet<TechSkillLog> TechSkillLogs { get; set; }
        // ── KNOWLEDGE BASE, LINKS, VAULT (NEW) ───────────────────────────
        public DbSet<KnowledgeEntry> KnowledgeEntries { get; set; }
        public DbSet<WebLink> WebLinks { get; set; }
        public DbSet<VaultEntry> VaultEntries { get; set; }
        public DbSet<MoneyTransaction> MoneyTransactions { get; set; }
        public DbSet<MonthlyBudget> MonthlyBudgets { get; set; }
        public DbSet<MoneyAccount> MoneyAccounts { get; set; }
        public DbSet<MoneyLoan> MoneyLoans { get; set; }
        public DbSet<LoanRepayment> LoanRepayments { get; set; }
        public DbSet<AccountTransfer> AccountTransfers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MoneyTransaction>().Property(t => t.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<MonthlyBudget>().Property(t => t.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<MonthlyBudget>().HasIndex(b => b.Month).IsUnique();

            modelBuilder.Entity<MoneyAccount>().Property(a => a.OpeningBalance).HasPrecision(18, 2);
            modelBuilder.Entity<MoneyLoan>().Property(a => a.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<LoanRepayment>().Property(a => a.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<AccountTransfer>().Property(a => a.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<MoneyTransaction>().HasOne(t => t.MoneyAccount).WithMany().HasForeignKey(t => t.MoneyAccountId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<MoneyLoan>().HasOne(t => t.MoneyAccount).WithMany().HasForeignKey(t => t.MoneyAccountId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<LoanRepayment>().HasOne(t => t.MoneyLoan).WithMany(l => l.Repayments).HasForeignKey(t => t.MoneyLoanId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LoanRepayment>().HasOne(t => t.MoneyAccount).WithMany().HasForeignKey(t => t.MoneyAccountId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AccountTransfer>().HasOne(t => t.FromAccount).WithMany().HasForeignKey(t => t.FromAccountId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AccountTransfer>().HasOne(t => t.ToAccount).WithMany().HasForeignKey(t => t.ToAccountId).OnDelete(DeleteBehavior.Restrict);
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
