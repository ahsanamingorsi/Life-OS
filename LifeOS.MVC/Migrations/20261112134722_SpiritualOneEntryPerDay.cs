using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class SpiritualOneEntryPerDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SpiritualEntries_Date",
                table: "SpiritualEntries",
                column: "Date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SpiritualEntries_Date",
                table: "SpiritualEntries");
        }
    }
}
