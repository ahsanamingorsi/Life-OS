using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMoneyAndKnowledge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountTransfers");

            migrationBuilder.DropTable(
                name: "KnowledgeEntries");

            migrationBuilder.DropTable(
                name: "LoanRepayments");

            migrationBuilder.DropTable(
                name: "MoneyTransactions");

            migrationBuilder.DropTable(
                name: "MonthlyBudgets");

            migrationBuilder.DropTable(
                name: "MoneyLoans");

            migrationBuilder.DropTable(
                name: "MoneyAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnowledgeEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoneyAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kind = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoneyAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyBudgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Month = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyBudgets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromAccountId = table.Column<int>(type: "int", nullable: false),
                    ToAccountId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountTransfers_MoneyAccounts_FromAccountId",
                        column: x => x.FromAccountId,
                        principalTable: "MoneyAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountTransfers_MoneyAccounts_ToAccountId",
                        column: x => x.ToAccountId,
                        principalTable: "MoneyAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MoneyLoans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoneyAccountId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Person = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoneyLoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoneyLoans_MoneyAccounts_MoneyAccountId",
                        column: x => x.MoneyAccountId,
                        principalTable: "MoneyAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MoneyTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoneyAccountId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoneyTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoneyTransactions_MoneyAccounts_MoneyAccountId",
                        column: x => x.MoneyAccountId,
                        principalTable: "MoneyAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoanRepayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoneyAccountId = table.Column<int>(type: "int", nullable: true),
                    MoneyLoanId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRepayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanRepayments_MoneyAccounts_MoneyAccountId",
                        column: x => x.MoneyAccountId,
                        principalTable: "MoneyAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanRepayments_MoneyLoans_MoneyLoanId",
                        column: x => x.MoneyLoanId,
                        principalTable: "MoneyLoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransfers_FromAccountId",
                table: "AccountTransfers",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTransfers_ToAccountId",
                table: "AccountTransfers",
                column: "ToAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRepayments_MoneyAccountId",
                table: "LoanRepayments",
                column: "MoneyAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRepayments_MoneyLoanId",
                table: "LoanRepayments",
                column: "MoneyLoanId");

            migrationBuilder.CreateIndex(
                name: "IX_MoneyLoans_MoneyAccountId",
                table: "MoneyLoans",
                column: "MoneyAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MoneyTransactions_MoneyAccountId",
                table: "MoneyTransactions",
                column: "MoneyAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyBudgets_Month",
                table: "MonthlyBudgets",
                column: "Month",
                unique: true);
        }
    }
}
