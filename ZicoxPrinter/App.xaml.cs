using CommunityToolkit.Maui.Core;
using System.Runtime.InteropServices;
using ZicoxPrinter.Services;

namespace ZicoxPrinter;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Debug.WriteLine("UnhandledException\tAppDomain:\t" + e.ExceptionObject.ToString());
            string message = AppResources.错误;
            if (e.ExceptionObject is Exception ex)
            {
                message = ex.Message;
            }
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, message, "OK");
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Debug.WriteLine("UnhandledException\tTaskScheduler:\t" + e.Exception.Message);
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, e.Exception.Message, "OK");
        };

        MainPage = new AppShell();

        if (Preferences.Default.ContainsKey(nameof(AppTheme)) && Current != null)
        {
            Current.UserAppTheme = Enum.TryParse<AppTheme>(Preferences.Default.Get(nameof(AppTheme), Current.RequestedTheme.ToString()), out var theme) ? theme : Current.RequestedTheme;
            SetStatusBar(Current.UserAppTheme);
        }
        else if (Current != null)
        {
            Current.UserAppTheme = Current.RequestedTheme;
            Preferences.Default.Set(nameof(AppTheme), Current.RequestedTheme.ToString());
            SetStatusBar(Current.RequestedTheme);
        }
        if (Current != null)
        {
            Current.RequestedThemeChanged += (s, a) =>
            {
                Debug.WriteLine("Current.RequestedThemeChanged:\t" + a.RequestedTheme);
                Current.UserAppTheme = a.RequestedTheme;
                Preferences.Default.Set(nameof(AppTheme), a.RequestedTheme.ToString());
                ICollection<ResourceDictionary> mergedDictionaries = Current.Resources.MergedDictionaries;
                if (mergedDictionaries != null)
                {
                    //mergedDictionaries.Clear();
                    //mergedDictionaries.Add();
                }
                SetStatusBar(a.RequestedTheme);
            };
        }
        Task.Run(() =>
        {
            CacheService.InitAllCacheDirectories();
        });
    }

    public static void SetStatusBar(AppTheme appTheme)
    {
#if ANDROID
        if (!OperatingSystem.IsAndroidVersionAtLeast(23, 0) || Current is null || Platform.CurrentActivity is null)
            return;
        if (appTheme == AppTheme.Light)
        {
            Current.Resources.TryGetValue("White", out var white);

            CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor((Color)white);
            CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.DarkContent);
        }
        else if (appTheme == AppTheme.Dark)
        {
            Current.Resources.TryGetValue("OffBlack", out var offBlack);

            CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor((Color)offBlack);
            CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.LightContent);
        }
        else
        {
            CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(StatusBarStyle.Default);
        }
#endif
    }
}
