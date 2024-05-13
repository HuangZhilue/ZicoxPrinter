using CommunityToolkit.Maui.Alerts;
using ZicoxPrinter.Services;

namespace ZicoxPrinter;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Task.Run(async () =>
        {
            Version? version = await AutoUpdate.GetNewVersion().ConfigureAwait(false);
            if (version is null) return;
#if DEBUG
            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Toast.Make($"{AppResources.当前版本}: {AppInfo.Current.VersionString}, {AppResources.最新版本}: {version}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            });
#endif
            bool needUpdate = await AutoUpdate.ReadyDownloadNewVersion(true).ConfigureAwait(false);
#if !DEBUG
            if (!needUpdate) return;
#endif
            await AutoUpdate.DownloadNewVersion().ConfigureAwait(false);

            Debug.WriteLine("Download complete!");
            // 安装更新包
            AutoUpdate.InstallNewVersion();
        });
    }
}
