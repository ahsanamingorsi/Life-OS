# Money accounts and loans
Open Money and use the section navigation:
- Transactions: record income and expenses against an account. Monthly income, spending, and budget totals exclude loans and transfers.
- Cash & online accounts: add Cash, Easypaisa, JazzCash, Bank, or Other accounts with a starting balance. Record transfers between accounts.
- Lending & borrowing: record the person, amount, date, optional due date, notes, and account. Lending reduces that account; borrowing increases it.
- Repayments: record a partial or full payment and select the account. Lending repayments increase that account; borrowing repayments reduce it. Payments above the outstanding balance are rejected.
Removing a loan, repayment, transfer, or transaction reverses its effect on calculated balances.
Balances include all recorded dates. Unassigned records do not affect a specific account. Accounts are manual trackers, not live integrations with financial providers.
Database migration: AddMoneyAccountsAndLoans. Existing transactions are preserved with no account assigned.
Verification: verify-money-loans.ps1 creates uniquely named temporary records and removes them after testing.
