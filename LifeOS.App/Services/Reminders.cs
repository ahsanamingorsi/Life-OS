using LifeOS.MVC.Data;
using LifeOS.MVC.Models;
using LifeOS.MVC.Services;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.App.Services
{
    /// <summary>Shows a system notification (Windows toast, later Android).</summary>
    public interface INotifier
    {
        void Show(string title, string body);
    }

    public sealed class NullNotifier : INotifier
    {
        public void Show(string title, string body) { }
    }

#if WINDOWS
    public sealed class WindowsNotifier : INotifier
    {
        // Unpackaged apps need an AppUserModelID registered for the current user, otherwise Windows drops toasts silently.
        private const string Aumid = "LifeOS.App";
        private bool _registered;

        public WindowsNotifier()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\AppUserModelId\" + Aumid);
                key.SetValue("DisplayName", "Life OS");
                var icon = Path.Combine(AppContext.BaseDirectory, "appicon.ico");
                if (File.Exists(icon)) key.SetValue("IconUri", icon);
                _registered = true;
            }
            catch (Exception ex) { _registered = false; Log("Register failed: " + ex); }
        }

        private static void Log(string text)
        {
            try { File.AppendAllText(Path.Combine(AppPaths.DataDirectory, "notifier.log"), DateTime.Now + " " + text + Environment.NewLine); }
            catch { }
        }

        public void Show(string title, string body)
        {
            if (!_registered) return;
            try
            {
                static string Esc(string s) => System.Security.SecurityElement.Escape(s) ?? "";
                var xml = new Windows.Data.Xml.Dom.XmlDocument();
                xml.LoadXml($"<toast><visual><binding template=\"ToastGeneric\"><text>{Esc(title)}</text><text>{Esc(body)}</text></binding></visual></toast>");
                Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier(Aumid)
                    .Show(new Windows.UI.Notifications.ToastNotification(xml));            }
            catch (Exception ex) { Log("Show failed: " + ex); }
        }
    }
#endif

#if ANDROID
    public sealed class AndroidNotifier : INotifier
    {
        private const string ChannelId = "lifeos_reminders";
        private static int _nextId = 1000;

        public void Show(string title, string body)
        {
            try
            {
                var ctx = Android.App.Application.Context;
                var nm = (Android.App.NotificationManager?)ctx.GetSystemService(Android.Content.Context.NotificationService);
                if (nm == null) return;
                if (OperatingSystem.IsAndroidVersionAtLeast(26))
                    nm.CreateNotificationChannel(new Android.App.NotificationChannel(ChannelId, "Reminders", Android.App.NotificationImportance.Default));

                var launch = ctx.PackageManager?.GetLaunchIntentForPackage(ctx.PackageName!);
                var pending = launch == null ? null : Android.App.PendingIntent.GetActivity(ctx, 0, launch, Android.App.PendingIntentFlags.Immutable | Android.App.PendingIntentFlags.UpdateCurrent);

                Android.App.Notification.Builder builder = OperatingSystem.IsAndroidVersionAtLeast(26)
                    ? new Android.App.Notification.Builder(ctx, ChannelId)
                    : new Android.App.Notification.Builder(ctx);
                builder.SetContentTitle(title).SetContentText(body).SetSmallIcon(ctx.ApplicationInfo!.Icon).SetAutoCancel(true);
                if (pending != null) builder.SetContentIntent(pending);
                nm.Notify(_nextId++, builder.Build());
            }
            catch { }
        }
    }
#endif

    /// <summary>"Start with Windows": a per-user Run entry that opens the app minimized.</summary>
    public static class AutoStart
    {
#if WINDOWS
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "LifeOS";
        public static bool Supported => true;

        public static bool IsEnabled()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunKey);
                return key?.GetValue(ValueName) != null;
            }
            catch { return false; }
        }

        public static void Set(bool enabled)
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
                if (key == null) return;
                if (enabled)
                {
                    var exe = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exe)) key.SetValue(ValueName, $"\"{exe}\" --minimized");
                }
                else key.DeleteValue(ValueName, throwOnMissingValue: false);
            }
            catch { }
        }
#else
        public static bool Supported => false;
        public static bool IsEnabled() => false;
        public static void Set(bool enabled) { }
#endif
    }

    /// <summary>Key / value settings stored in the AppSettings table.</summary>
    public static class SettingsStore
    {
        public static async Task<string> GetAsync(AppDbContext db, string key, string fallback)
        {
            var row = await db.AppSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key);
            return row?.Value ?? fallback;
        }

        public static async Task SetAsync(AppDbContext db, string key, string value)
        {
            var row = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (row == null) db.AppSettings.Add(new AppSetting { Key = key, Value = value });
            else row.Value = value;
            await db.SaveChangesAsync();
        }

        public static async Task<Dictionary<string, string>> AllAsync(AppDbContext db) =>
            await db.AppSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);
    }

    /// <summary>The reminder switches and times, with their defaults.</summary>
    public static class ReminderKeys
    {
        public const string Master = "rem.enabled";
        public static readonly string[] Prayers = { "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha" };
        public static readonly string[] PrayerDefaults = { "05:00", "12:30", "15:45", "18:00", "19:45" };
        public static string PrayerKey(string name) => "time.prayer." + name;

        public record Item(string Key, string Title, string DefaultTime, string Hint);
        public static readonly Item[] Items =
        {
            new("plan", "Plan your day", "08:00", "If you have no top 3 for today yet"),
            new("shutdown", "Evening shutdown", "21:30", "If you have not closed the day"),
            new("habit", "Habits check", "20:00", "If some habits are still open"),
            new("overdue", "Overdue tasks", "10:00", "If tasks are overdue"),
        };

        public static string OnKey(string item) => "rem.on." + item;
        public static string TimeKey(string item) => "time." + item;
        public const string PrayerOn = "rem.on.prayer";
        public const string BlockOn = "rem.on.block";
    }

    /// <summary>
    /// Checks every 30 seconds whether a reminder is due and shows it once.
    /// Works while the app is running (a minimized window is fine).
    /// </summary>
    public sealed class ReminderService : IDisposable
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly INotifier _notifier;
        private readonly HashSet<string> _fired = new();
        private System.Threading.Timer? _timer;
        private int _busy;

        public ReminderService(IServiceScopeFactory scopes, INotifier notifier)
        {
            _scopes = scopes;
            _notifier = notifier;
        }

        public void Start() => _timer ??= new System.Threading.Timer(_ => _ = CheckAsync(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));

        private static bool In(TimeSpan now, string hhmm)
        {
            if (!TimeSpan.TryParse(hhmm, out var t)) return false;
            return now >= t && now < t + TimeSpan.FromMinutes(20);
        }

        private bool Once(string id)
        {
            var key = DateTime.Today.ToString("yyyyMMdd") + "|" + id;
            return _fired.Add(key);
        }

        private async Task CheckAsync()
        {
            if (Interlocked.Exchange(ref _busy, 1) == 1) return;
            try
            {
                using var scope = _scopes.CreateScope();
                var sp = scope.ServiceProvider;
                var db = sp.GetRequiredService<AppDbContext>();
                var s = await SettingsStore.AllAsync(db);
                string G(string k, string d) => s.TryGetValue(k, out var v) ? v : d;
                bool On(string k, bool d = true) => G(k, d ? "1" : "0") == "1";

                if (!On(ReminderKeys.Master, false)) return;
                var now = DateTime.Now.TimeOfDay;
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // Prayers
                if (On(ReminderKeys.PrayerOn))
                {
                    var entry = await db.SpiritualEntries.AsNoTracking().FirstOrDefaultAsync(e => e.Date >= today && e.Date < tomorrow);
                    for (int i = 0; i < 5; i++)
                    {
                        string name = ReminderKeys.Prayers[i];
                        if (!In(now, G(ReminderKeys.PrayerKey(name), ReminderKeys.PrayerDefaults[i]))) continue;
                        bool done = entry != null && entry.Prayers[i];
                        if (!done && Once("prayer" + name)) _notifier.Show("Prayer time", $"It is time for {name}.");
                    }
                }

                // Time blocks: 5 minutes before they start
                if (On(ReminderKeys.BlockOn))
                {
                    int nowMin = (int)now.TotalMinutes;
                    var blocks = await db.PlanItems.AsNoTracking().Where(p => p.Date == today && !p.IsDone && p.StartMinutes != null).ToListAsync();
                    foreach (var b in blocks.Where(b => b.StartMinutes! - nowMin is >= 0 and <= 5))
                        if (Once("block" + b.Id)) _notifier.Show("Starting soon", $"{b.Title} at {PlanService.TimeText(b.StartMinutes!.Value)}");
                }

                // Daily nudges
                if (On(ReminderKeys.OnKey("plan")) && In(now, G(ReminderKeys.TimeKey("plan"), "08:00"))
                    && !await db.PlanItems.AnyAsync(p => p.Date == today && p.IsMit) && Once("plan"))
                    _notifier.Show("Plan your day", "Choose your 3 most important things for today.");

                if (On(ReminderKeys.OnKey("shutdown")) && In(now, G(ReminderKeys.TimeKey("shutdown"), "21:30"))
                    && !await db.DayReflections.AnyAsync(r => r.Date == today) && Once("shutdown"))
                    _notifier.Show("Close your day", "Do your evening shutdown and set tomorrow's top 3.");

                if (On(ReminderKeys.OnKey("habit")) && In(now, G(ReminderKeys.TimeKey("habit"), "20:00")) && Once("habit"))
                {
                    var habits = (await db.Habits.AsNoTracking().Where(h => !h.IsArchived).ToListAsync()).Where(h => HabitMath.IsActiveOn(h, today)).ToList();
                    var logs = await db.HabitLogs.AsNoTracking().Where(l => l.Date == today).ToDictionaryAsync(l => l.HabitId, l => l.Value);
                    int open = habits.Count(h => !HabitMath.IsDone(h, logs.GetValueOrDefault(h.Id)));
                    if (open > 0) _notifier.Show("Habits", $"{open} habit{(open == 1 ? "" : "s")} still open today.");
                }

                if (On(ReminderKeys.OnKey("overdue")) && In(now, G(ReminderKeys.TimeKey("overdue"), "10:00")) && Once("overdue"))
                {
                    var tasks = sp.GetRequiredService<TaskService>();
                    int overdue = (await tasks.GetForDayAsync(today, includeOverdue: true)).Count(t => !t.IsDone && t.IsOverdue);
                    if (overdue > 0) _notifier.Show("Overdue tasks", $"You have {overdue} overdue task{(overdue == 1 ? "" : "s")}.");
                }
            }
            catch
            {
                // A reminder problem must never crash the app.
            }
            finally { Interlocked.Exchange(ref _busy, 0); }
        }

        public void Dispose() => _timer?.Dispose();
    }
}
