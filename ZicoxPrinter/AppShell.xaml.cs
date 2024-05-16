using ZicoxPrinter.Services;

namespace ZicoxPrinter;

public partial class AppShell : Shell
{
    public static event EventHandler<ShellNavigatedEventArgs>? ShellNavigated;

    public AppShell()
    {
        InitializeComponent();
        Task.Run(async () =>
        {
            Version? version = await AutoUpdate.GetNewVersion().ConfigureAwait(false);
            if (version is null) return;
#if DEBUG
            ApplicationEx.ToastMakeOnUIThread($"{AppResources.当前版本}: {AppInfo.Current.VersionString}, {AppResources.最新版本}: {version}", CommunityToolkit.Maui.Core.ToastDuration.Long);
#endif
            bool needUpdate = await AutoUpdate.ReadyDownloadNewVersion(true).ConfigureAwait(false);
            //#if !DEBUG
            if (!needUpdate) return;
            //#endif
            await AutoUpdate.DownloadNewVersion().ConfigureAwait(false);

            Debug.WriteLine("Download complete!");
            // 安装更新包
            AutoUpdate.InstallNewVersion();
        });
    }

    private void Shell_Navigated(object sender, ShellNavigatedEventArgs e)
    {
        Debug.WriteLine($"sender:{((AppShell)sender).CurrentPage}{Environment.NewLine}ShellNavigated\tSource:{e.Source}, Current:{e.Current}, Previous:{e.Previous}");
        ShellNavigated?.Invoke(sender, e);
    }
}
