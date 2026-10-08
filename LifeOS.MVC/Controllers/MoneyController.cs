using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LifeOS.MVC.Controllers;
public class MoneyController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(DateTime? month)
    {
        var date = month ?? DateTime.Today;
        return View(await Load(new DateTime(date.Year, date.Month, 1)));
    }
    private async Task<MoneyViewModel> Load(DateTime month) => new()
    {
        Month = month,
        Accounts = await db.MoneyAccounts.AsNoTracking().OrderBy(a => a.Name).ToListAsync(),
        Transactions = await db.MoneyTransactions.AsNoTracking().Include(t => t.MoneyAccount).Where(t => t.Date >= month && t.Date < month.AddMonths(1)).OrderByDescending(t => t.Date).ThenByDescending(t => t.Id).ToListAsync(),
        Budget = await db.MonthlyBudgets.Where(b => b.Month == month).Select(b => b.Amount).FirstOrDefaultAsync()
    };
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Description,Type,Category,Amount,Date,MoneyAccountId")] MoneyTransaction entry)
    {
        if (entry.Date == default) ModelState.AddModelError("Entry.Date", "Choose a valid date.");
        if (!ModelState.IsValid)
        {
            var vm = await Load(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
            vm.Entry = entry;
            return View("Index", vm);
        }
        if (!await AccountExists(entry.MoneyAccountId)) return BadRequest("Account not found.");
        entry.Description = entry.Description.Trim();
        entry.Category = entry.Category.Trim();
        entry.Date = entry.Date.Date;
        db.MoneyTransactions.Add(entry);
        await db.SaveChangesAsync();
        TempData["Success"] = "Transaction saved.";
        return RedirectToAction("Index", new { month = entry.Date.ToString("yyyy-MM-01") });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Budget(DateTime month, decimal amount)
    {
        if (month == default || amount <= 0 || amount > 999999999999.99m)
        {
            TempData["Error"] = "Enter a valid month and a positive budget.";
            return RedirectToAction("Index");
        }
        month = new DateTime(month.Year, month.Month, 1);
        var budget = await db.MonthlyBudgets.SingleOrDefaultAsync(b => b.Month == month);
        if (budget == null) db.MonthlyBudgets.Add(new MonthlyBudget { Month = month, Amount = amount });
        else budget.Amount = amount;
        await db.SaveChangesAsync();
        TempData["Success"] = "Monthly budget updated.";
        return RedirectToAction("Index", new { month = month.ToString("yyyy-MM-dd") });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await db.MoneyTransactions.FindAsync(id);
        if (entry == null) return NotFound();
        db.MoneyTransactions.Remove(entry);
        await db.SaveChangesAsync();
        return RedirectToAction("Index", new { month = entry.Date.ToString("yyyy-MM-01") });
    }
    private Task<bool> AccountExists(int? id) => id.HasValue ? db.MoneyAccounts.AnyAsync(a => a.Id == id) : Task.FromResult(true);
    public async Task<IActionResult> Accounts()
    {
        var accounts = await db.MoneyAccounts.AsNoTracking().OrderBy(a => a.Kind).ThenBy(a => a.Name).ToListAsync();
        var transactions = await db.MoneyTransactions.AsNoTracking().ToListAsync();
        var loans = await db.MoneyLoans.AsNoTracking().Include(l => l.Repayments).ToListAsync();
        var transfers = await db.AccountTransfers.AsNoTracking().Include(t => t.FromAccount).Include(t => t.ToAccount).OrderByDescending(t => t.Date).ThenByDescending(t => t.Id).ToListAsync();
        var balances = accounts.ToDictionary(a => a.Id, a => a.OpeningBalance
            + transactions.Where(t => t.MoneyAccountId == a.Id).Sum(t => t.Type == "Income" ? t.Amount : -t.Amount)
            + loans.Where(l => l.MoneyAccountId == a.Id).Sum(l => l.Direction == "Borrowing" ? l.Amount : -l.Amount)
            + loans.Sum(l => l.Repayments.Where(r => r.MoneyAccountId == a.Id).Sum(r => l.Direction == "Lending" ? r.Amount : -r.Amount))
            + transfers.Where(t => t.ToAccountId == a.Id).Sum(t => t.Amount)
            - transfers.Where(t => t.FromAccountId == a.Id).Sum(t => t.Amount));
        return View(new AccountsViewModel { Accounts = accounts, Balances = balances, Transfers = transfers.Take(20).ToList() });
    }
    [HttpPost]
    public async Task<IActionResult> AddAccount([Bind("Name,Kind,OpeningBalance")] MoneyAccount account)
    {
        if (string.IsNullOrWhiteSpace(account.Name)) { TempData["Error"] = "Enter an account name."; return RedirectToAction("Accounts"); }
        account.Name = account.Name.Trim();
        db.MoneyAccounts.Add(account);
        await db.SaveChangesAsync();
        TempData["Success"] = "Account added.";
        return RedirectToAction("Accounts");
    }
    [HttpPost]
    public async Task<IActionResult> Transfer([Bind("FromAccountId,ToAccountId,Amount,Date")] AccountTransfer transfer)
    {
        if (transfer.Date == default || transfer.FromAccountId == transfer.ToAccountId || !await AccountExists(transfer.FromAccountId) || !await AccountExists(transfer.ToAccountId))
        { TempData["Error"] = "Choose two different accounts and a valid date."; return RedirectToAction("Accounts"); }
        db.AccountTransfers.Add(transfer);
        await db.SaveChangesAsync();
        TempData["Success"] = "Transfer recorded.";
        return RedirectToAction("Accounts");
    }
    public async Task<IActionResult> Loans() => View(new LoansViewModel
    {
        Accounts = await db.MoneyAccounts.AsNoTracking().OrderBy(a => a.Name).ToListAsync(),
        Loans = await db.MoneyLoans.AsNoTracking().Include(l => l.MoneyAccount).Include(l => l.Repayments).ThenInclude(r => r.MoneyAccount).OrderByDescending(l => l.Date).ThenByDescending(l => l.Id).ToListAsync()
    });
    [HttpPost]
    public async Task<IActionResult> AddLoan([Bind("Person,Direction,Amount,Date,DueDate,Notes,MoneyAccountId")] MoneyLoan loan)
    {
        if (string.IsNullOrWhiteSpace(loan.Person) || loan.Date == default || loan.DueDate < loan.Date || !await AccountExists(loan.MoneyAccountId))
        { TempData["Error"] = "Enter a person, valid account, and dates. Due date must be on or after the loan date."; return RedirectToAction("Loans"); }
        loan.Person = loan.Person.Trim();
        db.MoneyLoans.Add(loan);
        await db.SaveChangesAsync();
        TempData["Success"] = "Loan recorded.";
        return RedirectToAction("Loans");
    }
    [HttpPost]
    public async Task<IActionResult> Repay([Bind("MoneyLoanId,Amount,Date,MoneyAccountId")] LoanRepayment repayment)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var loan = await db.MoneyLoans.Include(l => l.Repayments).FirstOrDefaultAsync(l => l.Id == repayment.MoneyLoanId);
        if (loan == null) return NotFound();
        if (repayment.Date < loan.Date || repayment.Date == default || repayment.Amount > loan.Remaining || !await AccountExists(repayment.MoneyAccountId))
        { TempData["Error"] = "Enter a repayment within the outstanding amount, a valid account, and a date on or after the loan date."; return RedirectToAction("Loans"); }
        db.LoanRepayments.Add(repayment);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Success"] = "Repayment recorded.";
        return RedirectToAction("Loans");
    }
    [HttpPost]
    public async Task<IActionResult> DeleteRepayment(int id)
    {
        var repayment = await db.LoanRepayments.FindAsync(id);
        if (repayment == null) return NotFound();
        db.LoanRepayments.Remove(repayment);
        await db.SaveChangesAsync();
        TempData["Success"] = "Repayment removed and balances updated.";
        return RedirectToAction("Loans");
    }
    [HttpPost]
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var loan = await db.MoneyLoans.FindAsync(id);
        if (loan == null) return NotFound();
        db.MoneyLoans.Remove(loan);
        await db.SaveChangesAsync();
        TempData["Success"] = "Loan and repayments removed. Account balances updated.";
        return RedirectToAction("Loans");
    }
    [HttpPost]
    public async Task<IActionResult> DeleteTransfer(int id)
    {
        var transfer = await db.AccountTransfers.FindAsync(id);
        if (transfer == null) return NotFound();
        db.AccountTransfers.Remove(transfer);
        await db.SaveChangesAsync();
        return RedirectToAction("Accounts");
    }}
