using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using SkiaSharp;

namespace ZicoxPrinter.Services;

public static class ApplicationEx
{
    private static int ScreenWidth { get; set; } = 500;//1920;
    private static int ScreenHeight { get; set; } = 500;//1080;

    public static byte[] CreatePreviewImage(this Microsoft.Maui.Graphics.IImage image, byte[] bytes)
    {
        try
        {
            // 生成缩略图
            double scaleFactor = Math.Min(ScreenWidth / image.Width, ScreenHeight / image.Height);
            int newWidth = (int)(image.Width * scaleFactor);
            int newHeight = (int)(image.Height * scaleFactor);
            using SKBitmap originalBitmap = SKBitmap.Decode(bytes);
            using SKBitmap resizedBitmap = new(newWidth, newHeight);
            using (SKCanvas canvas = new(resizedBitmap))
            {
                canvas.DrawBitmap(originalBitmap, new SKRect(0, 0, newWidth, newHeight));
            }

            using SKData data = resizedBitmap.Encode(SKEncodedImageFormat.Jpeg, 80);
            return data.ToArray();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CreatePreviewImage Error: {ex.Message}");
            return [];
        }
        finally
        {
            GC.Collect();
        }
    }

    public static void DisplayAlertOnUIThread(string title, string message, string cancel)
    {
        if (Application.Current is null || Application.Current.MainPage is null) return;
        Application.Current.Dispatcher.Dispatch(() =>
        {
            _ = Application.Current.MainPage.DisplayAlert(title, message, cancel);
        });
    }

    public static async Task<bool> DisplayAlertOnUIThreadAsync(string title, string message, string accept, string cancel)
    {
        if (Application.Current is null || Application.Current.MainPage is null) return false;

        TaskCompletionSource<bool> tcs = new();
        Application.Current.Dispatcher.Dispatch(async () =>
        {
            bool r = await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel).ConfigureAwait(false);
            tcs.SetResult(r);
        });

        bool action = await tcs.Task.ConfigureAwait(false);
        return action;
    }

    public static async Task<string> DisplayActionSheetOnUIThreadAsync(string title, string? cancel, string? destruction, params string[] buttons)
    {
        if (Application.Current is null || Application.Current.MainPage is null) return string.Empty;

        TaskCompletionSource<string> tcs = new();
        Application.Current.Dispatcher.Dispatch(async () =>
        {
            string action = await Application.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons).ConfigureAwait(false);
            tcs.SetResult(action);
        });
        string action = await tcs.Task.ConfigureAwait(false);
        return action;
    }

    public static void ToastMakeOnUIThread(string message, ToastDuration duration = ToastDuration.Short, double textSize = 14.0)
    {
        if (Application.Current is null || Application.Current.MainPage is null) return;
        Application.Current.Dispatcher.Dispatch(() =>
        {
            _ = Toast.Make(message, duration, textSize).Show();
        });
    }
}
