using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class RepeatedTasksGoalsRemoveHabits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HabitEntries");

            migrationBuilder.DropTable(
                name: "HabitLogs");

            migrationBuilder.DropTable(
                name: "HealthEntries");

            migrationBuilder.DropTable(
                name: "Streaks");

            migrationBuilder.DropTable(
                name: "Habits");

            // Habits are gone: remove their activity history too (user decision, no backup).
            migrationBuilder.Sql("DELETE FROM [ActivityLogs] WHERE [Type] = N'Habit';");

            migrationBuilder.AddColumn<int>(
                name: "GoalId",
                table: "PersonalTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Repeat",
                table: "PersonalTasks",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "None"); // existing tasks become one-time tasks

            migrationBuilder.AddColumn<int>(
                name: "RepeatDayOfMonth",
                table: "PersonalTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatDays",
                table: "PersonalTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepeatEndDate",
                table: "PersonalTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetCount",
                table: "PersonalGoals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaskCompletions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonalTaskId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "IX_TaskCompletions_PersonalTaskId_Date",
                table: "TaskCompletions",
                columns: new[] { "PersonalTaskId", "Date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalTasks_PersonalGoals_GoalId",
                table: "PersonalTasks",
                column: "GoalId",
                principalTable: "PersonalGoals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalTasks_PersonalGoals_GoalId",
                table: "PersonalTasks");

            migrationBuilder.DropTable(
                name: "TaskCompletions");

            migrationBuilder.DropIndex(
                name: "IX_PersonalTasks_GoalId",
                table: "PersonalTasks");

            migrationBuilder.DropColumn(
                name: "GoalId",
                table: "PersonalTasks");

            migrationBuilder.DropColumn(
                name: "Repeat",
                table: "PersonalTasks");

            migrationBuilder.DropColumn(
                name: "RepeatDayOfMonth",
                table: "PersonalTasks");

            migrationBuilder.DropColumn(
                name: "RepeatDays",
                table: "PersonalTasks");

            migrationBuilder.DropColumn(
                name: "RepeatEndDate",
                table: "PersonalTasks");

            migrationBuilder.DropColumn(
                name: "TargetCount",
                table: "PersonalGoals");

            migrationBuilder.CreateTable(
                name: "HabitEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommunicationPractice = table.Column<bool>(type: "bit", nullable: false),
                    CreativityTask = table.Column<bool>(type: "bit", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogicTask = table.Column<bool>(type: "bit", nullable: false),
                    PagesRead = table.Column<int>(type: "int", nullable: false),
                    ReadingNote = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReflectionNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Habits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HealthEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DietClean = table.Column<bool>(type: "bit", nullable: false),
                    DietNote = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EnergyLevel = table.Column<int>(type: "int", nullable: false),
                    SleepHours = table.Column<double>(type: "float", nullable: false),
                    WaterLiters = table.Column<double>(type: "float", nullable: false),
                    WeightKg = table.Column<double>(type: "float", nullable: true),
                    WorkoutDone = table.Column<bool>(type: "bit", nullable: false),
                    WorkoutNote = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HabitLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HabitId = table.Column<int>(type: "int", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HabitLogs_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Streaks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HabitId = table.Column<int>(type: "int", nullable: false),
                    BestStreak = table.Column<int>(type: "int", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    LastCompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Streaks_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HabitLogs_HabitId_CompletedDate",
                table: "HabitLogs",
                columns: new[] { "HabitId", "CompletedDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Streaks_HabitId",
                table: "Streaks",
                column: "HabitId",
                unique: true);
        }
    }
}
