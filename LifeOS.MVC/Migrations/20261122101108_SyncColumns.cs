using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class SyncColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "WebLinks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "WebLinks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "TechSkills",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "TechSkills",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "TechSkillLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "TechSkillLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "TaskCompletions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "TaskCompletions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "StudyTasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "StudyTasks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "StudySubjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "StudySubjects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "StudySessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "StudySessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "StudyNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "StudyNotes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "SpiritualEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "SpiritualEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "ReviewEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "ReviewEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "ProjectTasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "ProjectTasks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "Projects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "PlanItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "PlanItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "PersonalTasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "PersonalTasks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "PersonalGoals",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "PersonalGoals",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "Habits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "Habits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "HabitLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "HabitLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "FocusSessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "FocusSessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "SyncId",
                table: "DayReflections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "UpdatedTicks",
                table: "DayReflections",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "SyncTombstones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SyncId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeletedTicks = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncTombstones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebLinks_SyncId",
                table: "WebLinks",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_TechSkills_SyncId",
                table: "TechSkills",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_TechSkillLogs_SyncId",
                table: "TechSkillLogs",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCompletions_SyncId",
                table: "TaskCompletions",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyTasks_SyncId",
                table: "StudyTasks",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySubjects_SyncId",
                table: "StudySubjects",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_SyncId",
                table: "StudySessions",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyNotes_SyncId",
                table: "StudyNotes",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_SpiritualEntries_SyncId",
                table: "SpiritualEntries",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewEntries_SyncId",
                table: "ReviewEntries",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_SyncId",
                table: "ProjectTasks",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SyncId",
                table: "Projects",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanItems_SyncId",
                table: "PlanItems",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalTasks_SyncId",
                table: "PersonalTasks",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalGoals_SyncId",
                table: "PersonalGoals",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_Habits_SyncId",
                table: "Habits",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_HabitLogs_SyncId",
                table: "HabitLogs",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_FocusSessions_SyncId",
                table: "FocusSessions",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_DayReflections_SyncId",
                table: "DayReflections",
                column: "SyncId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncTombstones_EntityType_SyncId",
                table: "SyncTombstones",
                columns: new[] { "EntityType", "SyncId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncTombstones");

            migrationBuilder.DropIndex(
                name: "IX_WebLinks_SyncId",
                table: "WebLinks");

            migrationBuilder.DropIndex(
                name: "IX_TechSkills_SyncId",
                table: "TechSkills");

            migrationBuilder.DropIndex(
                name: "IX_TechSkillLogs_SyncId",
                table: "TechSkillLogs");

            migrationBuilder.DropIndex(
                name: "IX_TaskCompletions_SyncId",
                table: "TaskCompletions");

            migrationBuilder.DropIndex(
                name: "IX_StudyTasks_SyncId",
                table: "StudyTasks");

            migrationBuilder.DropIndex(
                name: "IX_StudySubjects_SyncId",
                table: "StudySubjects");

            migrationBuilder.DropIndex(
                name: "IX_StudySessions_SyncId",
                table: "StudySessions");

            migrationBuilder.DropIndex(
                name: "IX_StudyNotes_SyncId",
                table: "StudyNotes");

            migrationBuilder.DropIndex(
                name: "IX_SpiritualEntries_SyncId",
                table: "SpiritualEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReviewEntries_SyncId",
                table: "ReviewEntries");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_SyncId",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_SyncId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_PlanItems_SyncId",
                table: "PlanItems");

            migrationBuilder.DropIndex(
                name: "IX_PersonalTasks_SyncId",
                table: "PersonalTasks");

            migrationBuilder.DropIndex(
                name: "IX_PersonalGoals_SyncId",
                table: "PersonalGoals");

            migrationBuilder.DropIndex(
                name: "IX_Habits_SyncId",
                table: "Habits");

            migrationBuilder.DropIndex(
                name: "IX_HabitLogs_SyncId",
                table: "HabitLogs");

            migrationBuilder.DropIndex(
                name: "IX_FocusSessions_SyncId",
                table: "FocusSessions");

            migrationBuilder.DropIndex(
                name: "IX_DayReflections_SyncId",
                table: "DayReflections");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "WebLinks");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "WebLinks");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "TechSkills");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "TechSkills");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "TechSkillLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "TechSkillLogs");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "TaskCompletions");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "TaskCompletions");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "StudyTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "StudyTasks");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "StudySubjects");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "StudySubjects");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "StudySessions");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "StudySessions");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "StudyNotes");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "StudyNotes");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "SpiritualEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "SpiritualEntries");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "ReviewEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "ReviewEntries");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "PlanItems");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "PlanItems");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "PersonalTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "PersonalTasks");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "PersonalGoals");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "PersonalGoals");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "HabitLogs");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "FocusSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "FocusSessions");

            migrationBuilder.DropColumn(
                name: "SyncId",
                table: "DayReflections");

            migrationBuilder.DropColumn(
                name: "UpdatedTicks",
                table: "DayReflections");
        }
    }
}
