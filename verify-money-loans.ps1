$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:5199'
$marker = 'Money-QA-' + [guid]::NewGuid().ToString('N')
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
function Query($sql) { $result = sqlcmd -S '.\SQLEXPRESS' -d LifeOSDb -E -C -h -1 -W -Q "SET NOCOUNT ON; $sql"; if ($LASTEXITCODE) { throw 'SQL failed' }; ($result | Where-Object {$_.Trim()}) -join '' }
function Post($action,$body,$page) {
 $html = (Invoke-WebRequest ($base+$page) -WebSession $session).Content
 $token = [regex]::Match($html,'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
 if (!$token) { throw 'Missing CSRF token' }
 $body['__RequestVerificationToken'] = $token
 Invoke-WebRequest ($base+'/Money/'+$action) -Method Post -Body $body -WebSession $session -Headers @{Referer=$base+$page} | Out-Null
}
function Assert($condition,$label) { if (!$condition) { throw "FAIL $label" }; "PASS $label" }
try {
 Post 'AddAccount' @{Name=$marker+' cash';Kind='Cash';OpeningBalance='1000'} '/Money/Accounts'
 Post 'AddAccount' @{Name=$marker+' wallet';Kind='Easypaisa';OpeningBalance='500'} '/Money/Accounts'
 $cash=[int](Query "SELECT Id FROM MoneyAccounts WHERE Name='$marker cash'")
 $wallet=[int](Query "SELECT Id FROM MoneyAccounts WHERE Name='$marker wallet'")
 Assert ($cash -gt 0 -and $wallet -gt 0) 'Cash and Easypaisa accounts persist'
 Post 'AddLoan' @{Person=$marker;Direction='Lending';Amount='200';Date='2026-10-08';MoneyAccountId=$cash} '/Money/Loans'
 $loan=[int](Query "SELECT Id FROM MoneyLoans WHERE Person='$marker'")
 Post 'Repay' @{MoneyLoanId=$loan;Amount='50';Date='2026-10-08';MoneyAccountId=$wallet} '/Money/Loans'
 Assert ((Query "SELECT SUM(Amount) FROM LoanRepayments WHERE MoneyLoanId=$loan") -eq '50.00') 'Partial repayment persists'
 Post 'Repay' @{MoneyLoanId=$loan;Amount='151';Date='2026-10-08';MoneyAccountId=$wallet} '/Money/Loans'
 Assert ((Query "SELECT COUNT(*) FROM LoanRepayments WHERE MoneyLoanId=$loan") -eq '1') 'Overpayment rejected'
 Post 'Repay' @{MoneyLoanId=$loan;Amount='0';Date='2026-10-08'} '/Money/Loans'
 Assert ((Query "SELECT COUNT(*) FROM LoanRepayments WHERE MoneyLoanId=$loan") -eq '1') 'Zero repayment rejected'
 Post 'Repay' @{MoneyLoanId=$loan;Amount='150';Date='2026-10-08';MoneyAccountId=$wallet} '/Money/Loans'
 Assert ((Query "SELECT SUM(Amount) FROM LoanRepayments WHERE MoneyLoanId=$loan") -eq '200.00') 'Full repayment settles loan'
 Post 'AddLoan' @{Person=$marker+' borrowed';Direction='Borrowing';Amount='300';Date='2026-10-08';MoneyAccountId=$wallet} '/Money/Loans'
 $borrow=[int](Query "SELECT Id FROM MoneyLoans WHERE Person='$marker borrowed'")
 Post 'Repay' @{MoneyLoanId=$borrow;Amount='100';Date='2026-10-08';MoneyAccountId=$cash} '/Money/Loans'
 Post 'Transfer' @{FromAccountId=$wallet;ToAccountId=$cash;Amount='100';Date='2026-10-08'} '/Money/Accounts'
 Post 'Transfer' @{FromAccountId=$cash;ToAccountId=$cash;Amount='100';Date='2026-10-08'} '/Money/Accounts'
 Assert ((Query "SELECT COUNT(*) FROM AccountTransfers WHERE FromAccountId=$wallet AND ToAccountId=$cash") -eq '1') 'Transfer saves and same-account transfer rejected'
 Post 'Create' @{'Entry.Description'=$marker;'Entry.Type'='Expense';'Entry.Category'='Other';'Entry.Amount'='25';'Entry.Date'='2026-10-08';'Entry.MoneyAccountId'=$wallet} '/Money'
 Assert ((Query "SELECT MoneyAccountId FROM MoneyTransactions WHERE Description='$marker'") -eq "$wallet") 'Expense linked to selected wallet'
 $html=(Invoke-WebRequest ($base+'/Money/Accounts') -WebSession $session).Content
 Assert ($html -match 'Rs 800.00' -and $html -match 'Rs 875.00') 'Balances include loans, repayments, transfers, and expenses correctly'
 Assert ((Query "SELECT COUNT(*) FROM MoneyTransactions WHERE Description='$marker'") -eq '1') 'Loans do not create income or expense records'
 $repayment=[int](Query "SELECT TOP 1 Id FROM LoanRepayments WHERE MoneyLoanId=$loan ORDER BY Id")
 Post 'DeleteRepayment' @{id=$repayment} '/Money/Loans'
 Assert ((Query "SELECT SUM(Amount) FROM LoanRepayments WHERE MoneyLoanId=$loan") -eq '150.00') 'Removing repayment reopens outstanding balance'
 Post 'DeleteLoan' @{id=$loan} '/Money/Loans'
 Assert ((Query "SELECT COUNT(*) FROM LoanRepayments WHERE MoneyLoanId=$loan") -eq '0') 'Loan deletion removes repayments'
 $transfer=[int](Query "SELECT Id FROM AccountTransfers WHERE FromAccountId=$wallet AND ToAccountId=$cash")
 Post 'DeleteTransfer' @{id=$transfer} '/Money/Accounts'
 Assert ((Query "SELECT COUNT(*) FROM AccountTransfers WHERE Id=$transfer") -eq '0') 'Transfer removal works'
} finally {
 Query "DELETE FROM MoneyLoans WHERE Person LIKE '$marker%'; DELETE FROM MoneyTransactions WHERE Description='$marker'; DELETE FROM AccountTransfers WHERE FromAccountId IN (SELECT Id FROM MoneyAccounts WHERE Name LIKE '$marker%') OR ToAccountId IN (SELECT Id FROM MoneyAccounts WHERE Name LIKE '$marker%'); DELETE FROM MoneyAccounts WHERE Name LIKE '$marker%';" | Out-Null
}
