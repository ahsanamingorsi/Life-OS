using System.ComponentModel.DataAnnotations;
namespace LifeOS.MVC.Models;
public class MoneyTransaction
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Description { get; set; } = "";
    [Required, RegularExpression("Income|Expense")] public string Type { get; set; } = "Expense";
    [Required, StringLength(50)] public string Category { get; set; } = "Other";
    [Range(typeof(decimal), "0.01", "999999999999.99")] public decimal Amount { get; set; }
    public int? MoneyAccountId { get; set; }
    public MoneyAccount? MoneyAccount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
}
public class MonthlyBudget
{
    public int Id { get; set; }
    public DateTime Month { get; set; }
    [Range(typeof(decimal), "0.01", "999999999999.99")] public decimal Amount { get; set; }
}
public class MoneyViewModel
{
    public DateTime Month { get; set; }
    public List<MoneyTransaction> Transactions { get; set; } = new();
    public MoneyTransaction Entry { get; set; } = new();
    public decimal Income => Transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
    public decimal Expenses => Transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
    public decimal Balance => Income - Expenses;
    public decimal Budget { get; set; }
    public List<MoneyAccount> Accounts { get; set; } = new();
}
