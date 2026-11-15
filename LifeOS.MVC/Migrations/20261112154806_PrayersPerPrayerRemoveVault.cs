using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class PrayersPerPrayerRemoveVault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VaultEntries");

            migrationBuilder.AddColumn<bool>(
                name: "Asr",
                table: "SpiritualEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Dhuhr",
                table: "SpiritualEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Fajr",
                table: "SpiritualEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Isha",
                table: "SpiritualEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Maghrib",
                table: "SpiritualEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Older days only stored a count: mark the first N prayers of the day as done (user decision).
            migrationBuilder.Sql(@"
UPDATE [SpiritualEntries] SET
    [Fajr]    = CASE WHEN [PrayersDone] >= 1 THEN 1 ELSE 0 END,
    [Dhuhr]   = CASE WHEN [PrayersDone] >= 2 THEN 1 ELSE 0 END,
    [Asr]     = CASE WHEN [PrayersDone] >= 3 THEN 1 ELSE 0 END,
    [Maghrib] = CASE WHEN [PrayersDone] >= 4 THEN 1 ELSE 0 END,
    [Isha]    = CASE WHEN [PrayersDone] >= 5 THEN 1 ELSE 0 END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Asr",
                table: "SpiritualEntries");

            migrationBuilder.DropColumn(
                name: "Dhuhr",
                table: "SpiritualEntries");

            migrationBuilder.DropColumn(
                name: "Fajr",
                table: "SpiritualEntries");

            migrationBuilder.DropColumn(
                name: "Isha",
                table: "SpiritualEntries");

            migrationBuilder.DropColumn(
                name: "Maghrib",
                table: "SpiritualEntries");

            migrationBuilder.CreateTable(
                name: "VaultEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PasswordEncoded = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RecoveryEmail = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorMethod = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaultEntries", x => x.Id);
                });
        }
    }
}
