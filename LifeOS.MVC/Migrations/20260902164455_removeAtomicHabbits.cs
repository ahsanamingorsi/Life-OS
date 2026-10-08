using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class removeAtomicHabbits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtomicHabitLogs");

            migrationBuilder.DropTable(
                name: "AtomicHabits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AtomicHabits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BadHabitReplacement = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BadHabitTrigger = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BestStreak = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Craving = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    EnvironmentDesign = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FrictionLevel = table.Column<int>(type: "int", nullable: false),
                    HabitStack = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Identity = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImmediateReward = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ImplementationIntention = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastCompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MakeItDifficult = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MakeItUnattractive = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MotivationRitual = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Temptation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TotalCount = table.Column<int>(type: "int", nullable: false),
                    TwoMinuteVersion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtomicHabits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AtomicHabitLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtomicHabitId = table.Column<int>(type: "int", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MoodAfter = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Resisted = table.Column<bool>(type: "bit", nullable: false),
                    WasFullVersion = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtomicHabitLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtomicHabitLogs_AtomicHabits_AtomicHabitId",
                        column: x => x.AtomicHabitId,
                        principalTable: "AtomicHabits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtomicHabitLogs_AtomicHabitId_CompletedDate",
                table: "AtomicHabitLogs",
                columns: new[] { "AtomicHabitId", "CompletedDate" },
                unique: true);
        }
    }
}
