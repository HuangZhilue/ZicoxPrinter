using Newtonsoft.Json;
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
    private bool isNewReleaseAvailable = false;
    [ObservableProperty]
    private bool isPreviewRelease = false;
    [ObservableProperty]
    private string newReleaseVersion = string.Empty;
    [ObservableProperty]
    private string newReleaseMessage = string.Empty;
    [ObservableProperty]
    private string newReleaseFileSize = string.Empty;
    [ObservableProperty]
    private bool isCheckingUpdate = false;
    [ObservableProperty]
    private bool isDownloadingUpdate = false;
    [ObservableProperty]
    private double downloadProgress = 0;
    [ObservableProperty]
    private string downloadProgressString = string.Empty;

    public ObservableCollection<string> AppThemes { get; } = [];

    private bool CanRefreshUI { get; set; } = true;
    private NewReleaseModel NewRelease { get; set; } = null!;

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
        string newReleaseString = Preferences.Default.Get(nameof(NewReleaseModel), string.Empty);
        NewRelease = JsonConvert.DeserializeObject<NewReleaseModel>(newReleaseString) ?? null!;
        SetNewRelease();

        AutoUpdate.IsCheckingUpdateChanged += (sender, change) =>
        {
            IsCheckingUpdate = change;
        };
        AutoUpdate.IsDownloadingUpdateChanged += (sender, change) =>
        {
            IsDownloadingUpdate = change;
            IsCheckingUpdate = change;
        };
        AutoUpdate.NewReleaseChanged += (sender, change) =>
        {
            NewRelease = change;
            IsDownloadingUpdate = false;
            DownloadProgress = 0;
            DownloadProgressString = string.Empty;
            SetNewRelease();
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
                    //AutoUpdate.InstallNewVersion();
                }

                if (!CanRefreshUI) return;
                CanRefreshUI = false;
                Task.Run(async () =>
                {
                    DownloadProgressString = $"{localP * 100:N2}%";
                    DownloadProgress = localP;
                    Debug.WriteLine($"Download progress: {localP * 100:N2}%");
                    await Task.Delay(100).ConfigureAwait(false);
                    CanRefreshUI = true;
                });
            }
            catch (Exception ex)
            {
                ApplicationEx.ToastMakeOnUIThread($"{AppResources.更新下载失败}: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long);
                Debug.WriteLine($"Download Error: {ex.Message}");
            }
        };
    }

    [RelayCommand]
    public async Task SetAppTheme()
    {
        if (Application.Current is null) return;

        string theme = await ApplicationEx.DisplayActionSheetOnUIThreadAsync(AppResources.应用主题, null, null, [.. AppThemes]).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(theme)) return;
        CurrentTheme = theme;
        Application.Current.Dispatcher.Dispatch(() =>
        {
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
        });
    }

    [RelayCommand]
    public async Task CheckVersion()
    {
        try
        {
            _ = await AutoUpdate.GetNewRelease().ConfigureAwait(false);
            Debug.WriteLine($"CheckVersion: {NewRelease.Version}");
        }
        catch (Exception ex)
        {
            ApplicationEx.ToastMakeOnUIThread($"{AppResources.检查更新失败}: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long);
            Debug.WriteLine($"CheckVersion Error: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task DownloadNewVersion()
    {
        try
        {
            await AutoUpdate.DownloadNewVersion(NewRelease).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ApplicationEx.ToastMakeOnUIThread($"{AppResources.下载更新失败}: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long);
            Debug.WriteLine($"DownloadNewVersion Error: {ex.Message}");
        }
    }

    [RelayCommand]
    public void InstallNewVersion()
    {
        try
        {
            AutoUpdate.InstallNewVersion(NewRelease);
        }
        catch (Exception ex)
        {
            ApplicationEx.ToastMakeOnUIThread($"{AppResources.安装更新失败}: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long);
            Debug.WriteLine($"InstallNewVersion Error: {ex.Message}");
        }
    }

    private void SetNewRelease()
    {
        if (NewRelease is null) return;

        Version newVersion = AutoUpdate.StringToVersion(NewRelease.TagName);
        if (newVersion is null) return;
        //#if !DEBUG
        if (newVersion > AppInfo.Current.Version)
        //#endif
        {
            IsNewReleaseAvailable = true;
            long fileSize = NewRelease.AssetSize;
            if (fileSize > 0)
            {
                NewReleaseFileSize = AutoUpdate.FormatFileSize(fileSize);
            }
        }

        NewReleaseVersion = newVersion.ToString();
        NewReleaseMessage = NewRelease.Body;
        IsPreviewRelease = NewRelease.Prerelease;
    }

    [RelayCommand]
    public async Task OpenGithub()
    {
        await Launcher.OpenAsync("https://github.com/HuangZhilue/ZicoxPrinter").ConfigureAwait(false);
    }
}