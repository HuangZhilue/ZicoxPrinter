#if ANDROID
using Android.Content;
#endif
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ZicoxPrinter.Services;

public static class AutoUpdate
{
    public static bool IsCheckingUpdate { get; private set; } = false;
    public static bool IsDownloadingUpdate { get; private set; } = false;
    public static bool IsInstallingUpdate { get; private set; } = false;

    public static event EventHandler<double>? DownloadProgressChanged;
    public static event EventHandler<bool>? IsCheckingUpdateChanged;
    public static event EventHandler<bool>? IsDownloadingUpdateChanged;
    public static event EventHandler<bool>? IsInstallingUpdateChanged;
    public static event EventHandler<NewReleaseModel>? NewReleaseChanged;

    public static async Task<NewReleaseModel?> GetNewRelease()
    {
        if (IsCheckingUpdate) return null;

        try
        {
            IsCheckingUpdate = true;
            IsCheckingUpdateChanged?.Invoke(null, IsCheckingUpdate);

            Octokit.GitHubClient client = new(new Octokit.ProductHeaderValue("ZicoxPrinter"));
            Octokit.Release NewRelease = await client.Repository.Release.GetLatest("HuangZhilue", "ZicoxPrinter");

            Debug.WriteLine(JsonConvert.SerializeObject(NewRelease));
            Debug.WriteLine($"The latest release is tagged at {NewRelease.TagName} and is named {NewRelease.Name}");

            Octokit.ReleaseAsset? assets = NewRelease.Assets.FirstOrDefault(a => a.Name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase));
            NewReleaseModel newReleaseModel = new()
            {
                TagName = NewRelease.TagName,
                Name = NewRelease.Name,
                Version = StringToVersion(NewRelease.TagName),
                Body = NewRelease.Body,
                Prerelease = NewRelease.Prerelease,
                PublishedAt = NewRelease.PublishedAt.HasValue ? NewRelease.PublishedAt.Value.DateTime : DateTime.MinValue,
                AssetSize = assets is null ? 0 : assets.Size,
                AssetFileName = assets is null ? string.Empty : assets.Name,
                AsseBrowserDownloadUrltSize = assets is null ? string.Empty : assets.BrowserDownloadUrl
            };
#if DEBUG
            newReleaseModel.TagName = "1.4.0";
            newReleaseModel.Name = "1.4.0";
            newReleaseModel.Version = StringToVersion("1.4.0");
            newReleaseModel.Prerelease = true;
#endif
            Preferences.Default.Set(nameof(NewReleaseModel), JsonConvert.SerializeObject(newReleaseModel) ?? string.Empty);
            NewReleaseChanged?.Invoke(null, newReleaseModel);

            return newReleaseModel;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetNewVersion Error: {ex.Message}");
            ApplicationEx.ToastMakeOnUIThread($"{AppResources.检查更新失败}: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long);
        }
        finally
        {
            IsCheckingUpdate = false;
            IsCheckingUpdateChanged?.Invoke(null, IsCheckingUpdate);
        }

        return null;
    }

    public static async Task<bool> ReadyDownloadNewVersion(NewReleaseModel newReleaseModel, bool checkIgnore = false)
    {
        if (IsCheckingUpdate || newReleaseModel is null) return false;

        try
        {
            IsCheckingUpdate = true;
            IsCheckingUpdateChanged?.Invoke(null, IsCheckingUpdate);

            if (newReleaseModel.Version <= AppInfo.Current.Version || Application.Current is null || Application.Current.MainPage is null)
                return false;
            if (checkIgnore)
            {
                string ignoreUpdate = Preferences.Default.Get("IgnoreUpdate", "0.0.0");
                Version ignoreVersion = new(ignoreUpdate);
                if (ignoreVersion >= newReleaseModel.Version) return false;
            }

            string action = await ApplicationEx.DisplayActionSheetOnUIThreadAsync($"{AppResources.发现新版本}: {newReleaseModel.Version}", null, null, AppResources.立即更新, AppResources.暂不更新, AppResources.忽略该更新).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(action) || action == AppResources.暂不更新) return false;
            if (action == AppResources.忽略该更新)
            {
                Preferences.Default.Set("IgnoreUpdate", newReleaseModel.Version.ToString());
                return false;
            }
            return action == AppResources.立即更新;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ReadyDownloadNewVersion Error: {ex.Message}");

            ApplicationEx.ToastMakeOnUIThread($"{AppResources.准备下载更新失败}: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long);
        }
        finally
        {
            IsCheckingUpdate = false;
            IsCheckingUpdateChanged?.Invoke(null, IsCheckingUpdate);
        }
        return false;
    }

    public static async Task DownloadNewVersion(NewReleaseModel newReleaseModel)
    {
        if (IsDownloadingUpdate || newReleaseModel is null) return;
        string newReleaseFilePath = string.Empty;
        try
        {
            string downloadUrl = newReleaseModel.AsseBrowserDownloadUrltSize;
            if (string.IsNullOrWhiteSpace(downloadUrl)) return;

            // 下载更新包 && 下载进度显示
            newReleaseFilePath = Path.Combine(CacheService.TempDataDirectory, newReleaseModel.AssetFileName); // 临时文件路径
            IsDownloadingUpdate = true;
            IsDownloadingUpdateChanged?.Invoke(null, IsDownloadingUpdate);

            await DownloadFileAsync(downloadUrl, newReleaseFilePath, (p) =>
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
                DownloadProgressChanged?.Invoke(null, 0);

                if (!string.IsNullOrWhiteSpace(newReleaseFilePath) && File.Exists(newReleaseFilePath))
                    File.Delete(newReleaseFilePath);
            }
            finally
            {
                newReleaseFilePath = null!;
            }

            Debug.WriteLine($"DownloadNewVersion Error: {ex.Message}");
            ApplicationEx.ToastMakeOnUIThread($"{AppResources.下载更新失败}: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long);
        }
        finally
        {
            IsDownloadingUpdate = false;
            IsDownloadingUpdateChanged?.Invoke(null, IsDownloadingUpdate);
        }
    }

    public static void InstallNewVersion(NewReleaseModel newReleaseModel)
    {
        if (newReleaseModel is null || IsInstallingUpdate) return;
        string apkFilePath = Path.Combine(CacheService.TempDataDirectory, newReleaseModel.AssetFileName); // 临时文件路径
        if (!File.Exists(apkFilePath)) return;
        Debug.WriteLine($"Installing new version... File from: {apkFilePath}");
        try
        {
#if ANDROID
            IsInstallingUpdate = true;
            IsInstallingUpdateChanged?.Invoke(null, IsInstallingUpdate);
            if (Application.Current is null || Application.Current.MainPage is null || Platform.CurrentActivity is null || Platform.AppContext.ApplicationContext is null) return;
            Java.IO.File apkFile = new(apkFilePath);

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
            ApplicationEx.ToastMakeOnUIThread($"{AppResources.安装更新失败}: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long);
        }
        finally
        {
            IsInstallingUpdate = false;
            IsInstallingUpdateChanged?.Invoke(null, IsInstallingUpdate);
        }
    }

    private static string GetNumbers(string input)
    {
        string pattern = @"\d+";
        MatchCollection matches = Regex.Matches(input, pattern);
        string[] numbersArray = matches.Cast<Match>().Select(m => m.Value).ToArray();
        return string.Join(".", numbersArray);
    }

    public static Version StringToVersion(string input)
    {
        try
        {
            return new(GetNumbers(input));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"StringToVersion Error: {ex}");
        }
        return null!;
    }

    public static string FormatFileSize(long fileSize)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;

        while (fileSize >= 1024 && order < sizes.Length - 1)
        {
            order++;
            fileSize /= 1024;
        }

        return $"{fileSize} {sizes[order]}";
    }

    private static async Task DownloadFileAsync(string url, string savePath, Action<double> progressCallback, CancellationToken cancellationToken)
    {
        // Step 1 : Get call
        using HttpResponseMessage response = await new HttpClient().GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"The request returned with HTTP status code {response.StatusCode}");

        // Step 3 : Get total of data
        long totalData = response.Content.Headers.ContentLength.GetValueOrDefault(-1L);
        if (File.Exists(savePath) && totalData == new FileInfo(savePath).Length)
        {
            progressCallback?.Invoke(1);
            return;
        }

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

public class NewReleaseModel
{
    public string Name { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public Version Version { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public bool Prerelease { get; set; } = false;
    public DateTime PublishedAt { get; set; } = DateTime.MinValue;
    public long AssetSize { get; set; } = 0;
    public string AssetFileName { get; set; } = string.Empty;
    public string AsseBrowserDownloadUrltSize { get; set; } = string.Empty;
}