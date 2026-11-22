using Android.App;
using Android.Content.PM;
using Android.OS;

namespace LifeOS.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // Android 13+ asks the user before an app may show notifications.
        if (OperatingSystem.IsAndroidVersionAtLeast(33)
            && CheckSelfPermission("android.permission.POST_NOTIFICATIONS") != Permission.Granted)
            RequestPermissions(new[] { "android.permission.POST_NOTIFICATIONS" }, 1);
    }
}
