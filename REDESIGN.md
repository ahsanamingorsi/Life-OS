# Life OS — current state (November 12, 2026)

Life OS is an ASP.NET Core MVC app (.NET 10) with SQL Server (`LifeOSDb` on `.\SQLEXPRESS`).

## Navigation

- **Plan:** Dashboard, My day, Tasks, Projects, Studies, Skills
- **Life:** Spiritual, Goals, Review
- **Library:** Links, Vault
- **Insights:** Analytics

Removed modules: Habits, Mind and Health (replaced by repeated tasks), Learning (Studies and Skills are separate), Money management and the Knowledge base. Their tables were dropped by the migrations `RepeatedTasksGoalsRemoveHabits` and `RemoveMoneyAndKnowledge`.

## Design

- New design built with the UI UX Pro Max skill: flat style, teal primary `#0F766E`, orange accent `#C2410C`, Plus Jakarta Sans.
- Light and dark themes (saved per browser) and a mobile layout with a slide-out sidebar.
- Styles live in `wwwroot/css/lifeos-v2.css`, loaded after the older `lifeos.css` and `workspace.css`.
- Done in the new design: shell (sidebar, top bar, focus timer popup), Dashboard, My day, Tasks and Goals. The other pages still use the older look.

## How things work

- **Productivity score (dashboard):** equal average of Tasks, Projects, Studies, Skills and Spiritual for the day. Areas with nothing to measure that day are left out. Goals and Review appear only as weekly area cards.
- **Tasks:** one-time or repeated (Daily, Weekly on chosen days, Monthly on a day of the month), with an optional end date. Each day of a repeated task is stored in `TaskCompletions`, which gives history and streaks. A task can be linked to a goal.
- **Goal progress:** no linked task → manual progress. Otherwise the average of the parts that exist: one-time tasks done ÷ all one-time tasks, and repeated completions ÷ the goal's target count.
- **Project progress:** calculated from project tasks; typed by hand only when the project has no tasks.
- **Study subject progress:** average of hours ÷ target hours and subject tasks done ÷ all subject tasks; manual only when neither exists. Subjects also have weekly target hours.
- **Spiritual:** one entry per day (unique index), past days can be logged, 30-day history, streak of consecutive complete days.
- **Vault:** passwords are encrypted with Windows DPAPI for the current Windows user (`Services/VaultCrypto.cs`). They are never written into the page; the Show and Copy buttons fetch one password at a time from `/Vault/Reveal`. Passwords cannot be read by another Windows user or after reinstalling Windows.
- **Links:** only `http` and `https` addresses are saved; `https://` is added when missing.

## Database setup

From the `LifeOS.MVC` directory:

```powershell
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet tool run dotnet-ef migrations list
dotnet tool run dotnet-ef database update
```

Several migrations drop tables. Review any pending migration before applying it. The connection string is `ConnectionStrings:DefaultConnection`; the environment variable `ConnectionStrings__DefaultConnection` can override it. Do not commit database credentials.

## Running and updating the desktop app

- The desktop "Life OS" icon runs `launcher\Start-LifeOS.ps1`: it starts the published server (`DesktopApp`, port 5199) if needed and opens an Edge app window. See `RUN-LIFE-OS.md`.
- To publish changes: run `Update Life OS.cmd`, then open the app from the desktop icon.

## Testing changes

- Build: `dotnet build` in `LifeOS.MVC`.
- Run a separate test server: `dotnet run --no-launch-profile --urls http://127.0.0.1:5190` (Razor views are compiled at build time, so rebuild and restart after view changes).
- Use clearly marked test records (for example titles starting with `ZZ TEST`) and delete them, and their activity log rows, after testing.
- `verify-study-tasks.ps1` is the only remaining check script.
