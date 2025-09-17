using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SuperdevMauiDemoApp
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ConfigChanges.ScreenSize |
                               ConfigChanges.Orientation |
                               ConfigChanges.UiMode |
                               ConfigChanges.ScreenLayout |
                               ConfigChanges.SmallestScreenSize |
                               ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
        }
    }
}