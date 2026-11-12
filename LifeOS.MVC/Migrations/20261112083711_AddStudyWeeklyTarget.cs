using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyWeeklyTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "WeeklyTargetHours",
                table: "StudySubjects",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeeklyTargetHours",
                table: "StudySubjects");
        }
    }
}
