using LifeOS.App.Services;
using LifeOS.Data.Sqlite;
using LifeOS.MVC.Data;
using LifeOS.MVC.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeOS.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>();

		builder.Services.AddMauiBlazorWebView();

		// Local database: one SQLite file per Windows user, in the app's data folder.
		var databasePath = Path.Combine(AppPaths.DataDirectory, SqliteDatabase.FileName);
		builder.Services.AddDbContext<AppDbContext>(options => SqliteDatabase.Configure(options, databasePath));

		// Shared Life OS logic (same code the website uses).
		builder.Services.AddScoped<TaskService>();
		builder.Services.AddScoped<ProductivityService>();
		builder.Services.AddScoped<DashboardService>();

		// App-wide focus timer.
		builder.Services.AddSingleton<FocusTimer>();
		builder.Services.AddSingleton<AppUi>();

		// Reminders (system notifications while the app is running).
#if WINDOWS
		builder.Services.AddSingleton<INotifier, WindowsNotifier>();
#elif ANDROID
		builder.Services.AddSingleton<INotifier, AndroidNotifier>();
#else
		builder.Services.AddSingleton<INotifier, NullNotifier>();
#endif
		builder.Services.AddSingleton<ReminderService>();

		// Phone <-> PC sync over Wi-Fi.
		builder.Services.AddSingleton<LifeOS.App.Services.Sync.SyncServer>();
		builder.Services.AddSingleton<LifeOS.App.Services.Sync.SyncClient>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();

		// Create the database on first run, or update it to the newest version.
		using (var scope = app.Services.CreateScope())
			SqliteDatabase.Migrate(scope.ServiceProvider.GetRequiredService<AppDbContext>());

		app.Services.GetRequiredService<ReminderService>().Start();
		_ = app.Services.GetRequiredService<LifeOS.App.Services.Sync.SyncServer>().StartIfEnabledAsync();
		app.Services.GetRequiredService<LifeOS.App.Services.Sync.SyncClient>().StartAuto();

		return app;
	}
}
