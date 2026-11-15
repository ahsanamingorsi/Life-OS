using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Detail = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonalGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Goal = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProgressPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    IsAchieved = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalGoals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    ProgressPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    NextTask = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Deadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Period = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Wins = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    MissedGoals = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ImprovementSuggestions = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    OverallScore = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpiritualEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Fajr = table.Column<bool>(type: "INTEGER", nullable: false),
                    Dhuhr = table.Column<bool>(type: "INTEGER", nullable: false),
                    Asr = table.Column<bool>(type: "INTEGER", nullable: false),
                    Maghrib = table.Column<bool>(type: "INTEGER", nullable: false),
                    Isha = table.Column<bool>(type: "INTEGER", nullable: false),
                    PrayersDone = table.Column<int>(type: "INTEGER", nullable: false),
                    PrayersTotal = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplineStreak = table.Column<int>(type: "INTEGER", nullable: false),
                    GratitudeNote = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ReflectionNote = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpiritualEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudySubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TargetHours = table.Column<int>(type: "INTEGER", nullable: false),
                    HoursLogged = table.Column<double>(type: "REAL", nullable: false),
                    WeeklyTargetHours = table.Column<double>(type: "REAL", nullable: false),
                    ProgressPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    ResourceUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CurrentTopic = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySubjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TechSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProgressPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    HoursLogged = table.Column<double>(type: "REAL", nullable: false),
                    WeeklyTargetHours = table.Column<double>(type: "REAL", nullable: false),
                    IsCurrent = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsNext = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResourceUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ResourceName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CurrentTopic = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechSkills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPinned = table.Column<bool>(type: "INTEGER", nullable: false),
                    VisitCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastVisited = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonalTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Repeat = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    RepeatDays = table.Column<int>(type: "INTEGER", nullable: false),
                    RepeatDayOfMonth = table.Column<int>(type: "INTEGER", nullable: true),
                    RepeatEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GoalId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalTasks_PersonalGoals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "PersonalGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTasks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudySubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsPinned = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyNotes_StudySubjects_StudySubjectId",
                        column: x => x.StudySubjectId,
                        principalTable: "StudySubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudySessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudySubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationHours = table.Column<double>(type: "REAL", nullable: false),
                    TopicCovered = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FocusRating = table.Column<int>(type: "INTEGER", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudySessions_StudySubjects_StudySubjectId",
                        column: x => x.StudySubjectId,
                        principalTable: "StudySubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudySubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyTasks_StudySubjects_StudySubjectId",
                        column: x => x.StudySubjectId,
                        principalTable: "StudySubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechSkillLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TechSkillId = table.Column<int>(type: "INTEGER", nullable: false),
                    LogDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Hours = table.Column<double>(type: "REAL", nullable: false),
                    TopicCovered = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    LoggedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechSkillLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechSkillLogs_TechSkills_TechSkillId",
                        column: x => x.TechSkillId,
                        principalTable: "TechSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskCompletions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonalTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCompletions_PersonalTasks_PersonalTaskId",
                        column: x => x.PersonalTaskId,
                        principalTable: "PersonalTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalTasks_GoalId",
                table: "PersonalTasks",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_ProjectId",
                table: "ProjectTasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SpiritualEntries_Date",
                table: "SpiritualEntries",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyNotes_StudySubjectId",
                table: "StudyNotes",
                column: "StudySubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_StudySubjectId",
                table: "StudySessions",
                column: "StudySubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyTasks_StudySubjectId",
                table: "StudyTasks",
                column: "StudySubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCompletions_PersonalTaskId_Date",
                table: "TaskCompletions",
                columns: new[] { "PersonalTaskId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechSkillLogs_TechSkillId",
                table: "TechSkillLogs",
                column: "TechSkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "ProjectTasks");

            migrationBuilder.DropTable(
                name: "ReviewEntries");

            migrationBuilder.DropTable(
                name: "SpiritualEntries");

            migrationBuilder.DropTable(
                name: "StudyNotes");

            migrationBuilder.DropTable(
                name: "StudySessions");

            migrationBuilder.DropTable(
                name: "StudyTasks");

            migrationBuilder.DropTable(
                name: "TaskCompletions");

            migrationBuilder.DropTable(
                name: "TechSkillLogs");

            migrationBuilder.DropTable(
                name: "WebLinks");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "StudySubjects");

            migrationBuilder.DropTable(
                name: "PersonalTasks");

            migrationBuilder.DropTable(
                name: "TechSkills");

            migrationBuilder.DropTable(
                name: "PersonalGoals");
        }
    }
}
