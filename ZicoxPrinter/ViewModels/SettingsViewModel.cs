using CommunityToolkit.Maui.Alerts;
using System.Collections.ObjectModel;
using ZicoxPrinter.Services;

namespace ZicoxPrinter.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private string currentTheme = AppResources.跟随系统;
    [ObservableProperty]
    private string appVersion = AppInfo.Current.VersionString;
    [ObservableProperty]
    private bool isCheckingUpdate = false;
    [ObservableProperty]
    private bool isDownloadingUpdate = false;
    [ObservableProperty]
    private double downloadProgress = 0;
    [ObservableProperty]
    private string downloadProgressString = string.Empty;

    public ObservableCollection<string> AppThemes { get; } = [];

    private bool CanRefeshUI { get; set; } = true;

    public SettingsViewModel()
    {
        AppThemes = new(
            Enum.GetValues(typeof(AppTheme))
            .Cast<AppTheme>()
            .Select(x =>
            {
                if (x == AppTheme.Unspecified)
                {
                    return AppResources.跟随系统;
                }
                return x.ToString();
            })
            .ToList());
        if (Preferences.Default.ContainsKey(nameof(AppTheme)))
        {
            AppTheme appTheme = Enum.TryParse<AppTheme>(Preferences.Default.Get(nameof(AppTheme), AppTheme.Unspecified.ToString()), out appTheme) ? appTheme : AppTheme.Unspecified;
            CurrentTheme = appTheme == AppTheme.Unspecified ? AppResources.跟随系统 : appTheme.ToString();
        }
        else
        {
            Preferences.Default.Set(nameof(AppTheme), AppTheme.Unspecified.ToString());
            CurrentTheme = AppResources.跟随系统;
        }

        IsCheckingUpdate = AutoUpdate.IsCheckingUpdate;
        IsDownloadingUpdate = AutoUpdate.IsDownloadingUpdate;
        AutoUpdate.IsCheckingUpdateChanged += (sender, change) =>
        {
            IsCheckingUpdate = change;
        };
        AutoUpdate.IsDownloadingUpdateChanged += (sender, change) =>
        {
            IsDownloadingUpdate = change;
        };
        AutoUpdate.DownloadProgressChanged += (sender, p) =>
        {
            try
            {
                double localP = p;

                IsDownloadingUpdate = localP < 1;
                if (localP == 1)
                {
                    DownloadProgressString = $"{localP * 100:N2}%";
                    DownloadProgress = localP;
                    Debug.WriteLine("Download complete!");
                    // 安装更新包
                    AutoUpdate.InstallNewVersion();
                }

                if (!CanRefeshUI) return;
                CanRefeshUI = false;
                Task.Run(async () =>
                {
                    DownloadProgressString = $"{localP * 100:N2}%";
                    DownloadProgress = localP;
                    Debug.WriteLine($"Download progress: {localP * 100:N2}%");
                    await Task.Delay(100).ConfigureAwait(false);
                    CanRefeshUI = true;
                });
            }
            catch (Exception ex)
            {
                Application.Current!.Dispatcher.Dispatch(() =>
                {
                    _ = Toast.Make($"{AppResources.更新下载失败}: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
                });
                Debug.WriteLine($"Download Error: {ex.Message}");
            }
        };
    }

    [RelayCommand]
    public void SetAppTheme()
    {
        if (Application.Current is null) return;
        if (CurrentTheme == AppResources.跟随系统)
        {
            Application.Current.UserAppTheme = AppTheme.Unspecified;
        }
        else if (CurrentTheme == AppTheme.Light.ToString())
        {
            Application.Current.UserAppTheme = AppTheme.Light;
        }
        else if (CurrentTheme == AppTheme.Dark.ToString())
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
        }

        Preferences.Default.Set(nameof(AppTheme), Application.Current.UserAppTheme.ToString());
    }

    [RelayCommand]
    public async Task CheckVersion()
    {
        try
        {
            //IsCheckingUpdate = true;
            //IsDownloadingUpdate = false;

            Version? version = await AutoUpdate.GetNewVersion().ConfigureAwait(false);
            if (version is null) return;

            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Toast.Make($"{AppResources.当前版本}: {AppInfo.Current.VersionString}, {AppResources.最新版本}: {version}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            });

            bool needUpdate = await AutoUpdate.ReadyDownloadNewVersion().ConfigureAwait(false);
#if !DEBUG
            if (!needUpdate) return;
#endif
            IsDownloadingUpdate = true;
            await AutoUpdate.DownloadNewVersion().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Toast.Make($"{AppResources.检查更新失败}: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            });
            Debug.WriteLine($"CheckVersion Error: {ex.Message}");
        }
        finally
        {
            //IsCheckingUpdate = false;
            //IsDownloadingUpdate = false;
        }
    }
}