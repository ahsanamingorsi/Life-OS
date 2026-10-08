using System.ComponentModel.DataAnnotations;
namespace LifeOS.MVC.Models;
public class MoneyAccount
{
    public int Id { get; set; }
    [Required, StringLength(80)] public string Name { get; set; } = "";
    [Required, RegularExpression("Cash|Easypaisa|JazzCash|Bank|Other")] public string Kind { get; set; } = "Cash";
    [Range(typeof(decimal), "-999999999999.99", "999999999999.99")] public decimal OpeningBalance { get; set; }
}
public class MoneyLoan
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Person { get; set; } = "";
    [Required, RegularExpression("Lending|Borrowing")] public string Direction { get; set; } = "Lending";
    [Range(typeof(decimal), "0.01", "999999999999.99")] public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public DateTime? DueDate { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
    public int? MoneyAccountId { get; set; }
    public MoneyAccount? MoneyAccount { get; set; }
    public List<LoanRepayment> Repayments { get; set; } = new();
    public decimal Remaining => Amount - Repayments.Sum(p => p.Amount);
}
public class LoanRepayment
{
    public int Id { get; set; }
    public int MoneyLoanId { get; set; }
    public MoneyLoan? MoneyLoan { get; set; }
    [Range(typeof(decimal), "0.01", "999999999999.99")] public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public int? MoneyAccountId { get; set; }
    public MoneyAccount? MoneyAccount { get; set; }
}
public class AccountTransfer
{
    public int Id { get; set; }
    public int FromAccountId { get; set; }
    public MoneyAccount? FromAccount { get; set; }
    public int ToAccountId { get; set; }
    public MoneyAccount? ToAccount { get; set; }
    [Range(typeof(decimal), "0.01", "999999999999.99")] public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
}
public class AccountsViewModel
{
    public List<MoneyAccount> Accounts { get; set; } = new();
    public Dictionary<int, decimal> Balances { get; set; } = new();
    public List<AccountTransfer> Transfers { get; set; } = new();
}
public class LoansViewModel
{
    public List<MoneyLoan> Loans { get; set; } = new();
    public List<MoneyAccount> Accounts { get; set; } = new();
}
