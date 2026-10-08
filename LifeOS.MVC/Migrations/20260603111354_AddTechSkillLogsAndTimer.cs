using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddTechSkillLogsAndTimer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentTopic",
                table: "TechSkills",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "TechSkills",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResourceName",
                table: "TechSkills",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResourceUrl",
                table: "TechSkills",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TechSkills",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "WeeklyTargetHours",
                table: "TechSkills",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "TechSkillLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TechSkillId = table.Column<int>(type: "int", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hours = table.Column<double>(type: "float", nullable: false),
                    TopicCovered = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LoggedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_TechSkillLogs_TechSkillId",
                table: "TechSkillLogs",
                column: "TechSkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TechSkillLogs");

            migrationBuilder.DropColumn(
                name: "CurrentTopic",
                table: "TechSkills");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "TechSkills");

            migrationBuilder.DropColumn(
                name: "ResourceName",
                table: "TechSkills");

            migrationBuilder.DropColumn(
                name: "ResourceUrl",
                table: "TechSkills");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TechSkills");

            migrationBuilder.DropColumn(
                name: "WeeklyTargetHours",
                table: "TechSkills");
        }
    }
}
