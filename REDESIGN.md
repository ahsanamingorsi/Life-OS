# Life OS redesign and money management

The shared layout supports a persisted light/dark theme, responsive glass surfaces, and a redesigned dashboard. Money management uses SQL Server through the existing AppDbContext. Amounts are in PKR.

## Database setup

From the LifeOS.MVC directory:

```powershell
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet tool run dotnet-ef migrations list
```

Verify the existing migrations are already applied before updating. The new `AddMoneyManagement` migration only creates `MoneyTransactions`, `MonthlyBudgets`, and a unique monthly budget index. Earlier existing migrations include table deletions; review any earlier pending migrations before applying them.

```powershell
dotnet tool run dotnet-ef database update
dotnet run
```

The SQL Server connection is configured by `ConnectionStrings:DefaultConnection`; an environment variable `ConnectionStrings__DefaultConnection` can override it. Do not commit database credentials.

## Verification

- Switch themes, reload, and navigate to confirm the preference persists.
- Open the dashboard and Money on desktop and mobile widths.
- Record income and expenses, filter by month, and confirm balances and spending categories.
- Set a monthly budget and check remaining/over-budget amounts.
- Submit a zero or negative amount: validation must reject it.
- Delete a transaction after confirmation and verify the totals update.
- Open and close the focus timer with Escape; the backdrop must close too.

Build verification completed with no errors. The database connection now uses the local .\SQLEXPRESS instance instead of an old machine name. AddMoneyManagement has been applied to LifeOSDb. HTTP checks for Dashboard, Money, Analytics, Projects, and Habits returned 200 successfully. Visual browser verification remains outstanding.


## Simplified workspace (October 7, 2026)

Primary navigation now contains Dashboard, My day, Projects, Habits, Learning, Money, and Spiritual. Extra modules have been removed from the main workspace; existing records and legacy routes are preserved. Skills and Study remain accessible through Learning, and the full task list through My day.

My day includes real date selection, overdue and undated open tasks, quick task capture, task completion, routine completion/undo, and current projects. Placeholder progress widgets and inactive day controls were replaced.

Shared workspace styling applies to both legacy and current card classes, all forms, page headers, tables, buttons, navigation, and modals. Manual light/dark selection persists. Focus timing uses a real deadline and persists in the browser tab across navigation.

Validation rejects blank required fields and out-of-range learning values. Project task counts now use a typed model. Reopened project tasks reactivate completed projects, and removing study sessions recalculates hours and progress. Daily habit totals exclude inactive habits, prayer capacity is reported correctly, and expired streaks display zero.

Verification: isolated build succeeds with zero errors (five existing warnings). Eleven retained pages return HTTP 200. Fifteen form/database checks passed using temporary records, which were cleaned up. Light/dark surfaces and the responsive My day layout were visually inspected; timer navigation persistence and Escape closure were verified in the browser. Run `verify-workflows.ps1` only against a local test instance on port 5190; it creates and removes clearly marked temporary records in LifeOSDb.

If Visual Studio has the app running, stop and restart it to load the updated Razor views and controllers. An isolated `.verification` build was used because the existing running app locks the normal output executable.
