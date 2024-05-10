#if ANDROID
using Android.Content;
#endif
using CommunityToolkit.Maui.Alerts;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ZicoxPrinter.Services;

public class AutoUpdate
{
    public static bool IsCheckingUpdate { get; private set; } = false;
    public static bool IsDownloadingUpdate { get; private set; } = false;

    public static event EventHandler<double>? DownloadProgressChanged;

    private static Octokit.Release NewRelease { get; set; } = null!;
    private static Version NewVersion { get; set; } = null!;
    private static string NewReleaseFilePath { get; set; } = string.Empty;

    public static async Task<Version?> GetNewVersion()
    {
        if (IsCheckingUpdate) return null;

        try
        {
            IsCheckingUpdate = true;

            Octokit.GitHubClient client = new(new Octokit.ProductHeaderValue("ZicoxPrinter"));
            NewRelease = await client.Repository.Release.GetLatest("HuangZhilue", "ZicoxPrinter");
            Debug.WriteLine(JsonConvert.SerializeObject(NewRelease));
            Debug.WriteLine($"The latest release is tagged at {NewRelease.TagName} and is named {NewRelease.Name}");

            string v = GetNumbers(NewRelease.TagName);
            NewVersion = new(v);
            return NewVersion;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetNewVersion Error: {ex.Message}");

            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Toast.Make($"检查更新失败：{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            });
        }
        finally
        {
            IsCheckingUpdate = false;
        }

        return null;
    }

    public static async Task<bool> ReadyDownloadNewVersion()
    {
        if (IsCheckingUpdate || NewRelease is null || NewVersion is null) return false;

        try
        {
            IsCheckingUpdate = true;

            if (NewVersion <= AppInfo.Current.Version || Application.Current is null || Application.Current.MainPage is null) return false;

            TaskCompletionSource<string> tcs = new();
            Application.Current!.Dispatcher.Dispatch(async () =>
            {
                string action = await Application.Current!.MainPage!.DisplayActionSheet("发现新版本：" + NewVersion.ToString(), null, null, "立即更新", "暂不更新", "忽略该更新");
                tcs.SetResult(action);
            });
            string action = await tcs.Task;

            if (string.IsNullOrWhiteSpace(action) || action == "暂不更新") return false;
            if (action == "忽略该更新")
            {
                Preferences.Default.Set("IgnoreUpdate", NewVersion.ToString());
                return false;
            }
            return action == "立即更新";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ReadyDownloadNewVersion Error: {ex.Message}");

            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Toast.Make($"准备下载更新失败：{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            });
        }
        finally
        {
            IsCheckingUpdate = false;
        }
        return false;
    }

    public static async Task DownloadNewVersion()
    {
        if (IsDownloadingUpdate || NewRelease is null || NewVersion is null) return;
        try
        {
            Octokit.ReleaseAsset? assets = NewRelease.Assets.FirstOrDefault(a => a.Name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase));
            if (assets is null) return;

            string downloadUrl = assets.BrowserDownloadUrl;
            if (string.IsNullOrWhiteSpace(downloadUrl)) return;

            // 下载更新包 && 下载进度显示
            NewReleaseFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, "temp", assets.Name); // 临时文件路径
            IsDownloadingUpdate = true;
            await DownloadFileAsync(downloadUrl, NewReleaseFilePath, (p) =>
            {
                double localP = p;
                Debug.WriteLine($"Download progress: {localP * 100:N2}%");
                DownloadProgressChanged?.Invoke(null, localP);
            }, CancellationToken.None);

            Debug.WriteLine("Download complete!");
        }
        catch (Exception ex)
        {
            try
            {
                File.Delete(NewReleaseFilePath);
            }
            finally
            {
                NewReleaseFilePath = string.Empty;
            }

            Debug.WriteLine($"DownloadNewVersion Error: {ex.Message}");
            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Toast.Make($"下载更新失败：{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            });
        }
        finally
        {
            IsDownloadingUpdate = false;
        }
    }

    public static void InstallNewVersion()
    {
        if (!File.Exists(NewReleaseFilePath)) return;
        try
        {
#if ANDROID
            if (Application.Current is null || Application.Current.MainPage is null || Platform.CurrentActivity is null || Platform.AppContext.ApplicationContext is null) return;
            Java.IO.File apkFile = new(NewReleaseFilePath);

            // Use the FileProvider to get a content URI for the file
            Android.Net.Uri? apkUri = AndroidX.Core.Content.FileProvider.GetUriForFile(Android.App.Application.Context, "[Project.Application].fileprovider", apkFile);

            // Create an Intent to install the APK
            Intent intent = new(Intent.ActionView);
            intent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);

            // Start the installation process
            Android.App.Application.Context.StartActivity(intent);
#endif
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"InstallNewVersion Error: {ex.Message}");
            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Toast.Make($"安装更新失败：{ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            });
        }
        finally
        {
            //IsDownloadingUpdate = false;
        }
    }

    private static string GetNumbers(string input)
    {
        string pattern = @"\d+";
        MatchCollection matches = Regex.Matches(input, pattern);
        string[] numbersArray = matches.Cast<Match>().Select(m => m.Value).ToArray();
        return string.Join(".", numbersArray);
    }

    private static async Task DownloadFileAsync(string url, string savePath, Action<double> progressCallback, CancellationToken cancellationToken)
    {
        if (File.Exists(savePath)) return;

        // Step 1 : Get call
        using HttpResponseMessage response = await new HttpClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"The request returned with HTTP status code {response.StatusCode}");

        // Step 3 : Get total of data
        long totalData = response.Content.Headers.ContentLength.GetValueOrDefault(-1L);
        bool canSendProgress = totalData != -1L;

        // Step 5 : Download data
        using FileStream fileStream = new(savePath, FileMode.Create);
        using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        long totalRead = 0L;
        byte[] buffer = new byte[8192];
        bool isMoreDataToRead = true;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            int read = await stream.ReadAsync(buffer, cancellationToken);

            if (read == 0)
            {
                isMoreDataToRead = false;
            }
            else
            {
                // Write data on disk.
                await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);

                totalRead += read;

                if (canSendProgress)
                {
                    progressCallback?.Invoke((totalRead * 1d) / (totalData * 1d));
                }
            }
        } while (isMoreDataToRead);
        progressCallback?.Invoke((totalRead * 1d) / (totalData * 1d));
    }
}
