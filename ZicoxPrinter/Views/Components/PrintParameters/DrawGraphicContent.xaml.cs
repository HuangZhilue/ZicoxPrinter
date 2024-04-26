using Microsoft.Maui.Graphics.Platform;
using System.Collections.ObjectModel;
using ZicoxPrinter.Services.PrinterSDK;

namespace ZicoxPrinter.Views.Components.PrintParameters;

public partial class DrawGraphicContent : ContentView
{
    public ObservableCollection<DitheringType> DitheringTypes { get; set; } = new(Enum.GetValues(typeof(DitheringType)).Cast<DitheringType>());

    public static readonly BindableProperty ParametersProperty = BindableProperty.Create(nameof(Parameters), typeof(DrawGraphicParameters), typeof(DrawGraphicParameters), new DrawGraphicParameters());

    public DrawGraphicParameters Parameters
    {
        get => (DrawGraphicParameters)GetValue(ParametersProperty);
        set => SetValue(ParametersProperty, value);
    }

    public event EventHandler<EventArgs> ParametersChanged = null!;

    public string TipsBmpSizeW { get; set; } = string.Empty;
    public string TipsBmpSizeH { get; set; } = string.Empty;
    public ImageSource Image { get; set; } = null!;
    public ImageSource PreviewImage { get; set; } = null!;
    public Microsoft.Maui.Graphics.IImage IPreviewImage { get; set; } = null!;
    public int ImageHeight { get; set; } = 0;
    public int ImageWidth { get; set; } = 0;

    public DrawGraphicContent()
    {
        InitializeComponent();
    }

    public event EventHandler<EventArgs> ParametersRemoving = null!;

    [RelayCommand]
    public void RemoveThis()
    {
        ParametersRemoving?.Invoke(Parameters, EventArgs.Empty);
    }

    [RelayCommand]
    public async Task PickAndShow()
    {
        try
        {
            FileResult? photo = await MediaPicker.PickPhotoAsync();
            if (photo == null) return;
            string newFile = Path.Combine(FileSystem.CacheDirectory, "image_manager_disk_cache", photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
                stream.Position = 0;
                Microsoft.Maui.Graphics.IImage image = PlatformImage.FromStream(stream);
                ImageHeight = (int)image.Height;
                ImageWidth = (int)image.Width;
            }

            Image = ImageSource.FromFile(newFile);

            using MemoryStream memoryStream = new();
            using FileStream fileStream = File.OpenRead(newFile);
            fileStream.CopyTo(memoryStream);
            byte[] bytes = memoryStream.ToArray();
            Parameters.Base64 = Convert.ToBase64String(bytes);
            await PrinterImagePreview().ConfigureAwait(false);
            OnPropertyChanged(nameof(Image));
            OnPropertyChanged(nameof(ImageHeight));
            OnPropertyChanged(nameof(ImageWidth));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FilePicker Error: {ex.Message}");
            // The user canceled or something went wrong
            if (Application.Current is null || Application.Current.MainPage is null) return;
            _ = Application.Current.MainPage.DisplayAlert("错误", ex.Message, "OK");
        }
    }

    private CancellationTokenSource CancellationTokenSource { get; set; } = null!;
    [RelayCommand]
    public async Task PrinterImagePreview()
    {
        try
        {
            try
            {
                CancellationTokenSource?.Cancel();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PrinterImagePreview Error: {ex.Message}");
            }

            CancellationTokenSource = new CancellationTokenSource();

            // 处理请求的逻辑
            await Task.Delay(1000);

            // 检查请求是否已被取消
            CancellationTokenSource.Token.ThrowIfCancellationRequested();

            await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(Parameters.Base64)) return;
#if ANDROID
                    string? previewImage = Com.Api.MyZpSDK.PrinterCore.ProcessImageToBase64(
                        -1,
                        -1,
                        Parameters.StartX,
                        Parameters.StartY,
                        Parameters.BmpSizeWPercentage,
                        Parameters.BmpSizeHPercentage,
                        Parameters.Rotate,
                        Parameters.Threshold,
                        Com.Api.MyZpSDK.PrinterCore.DitheringType.Values()![(int)Parameters.DitheringType],
                        Parameters.Base64);
                    if (string.IsNullOrWhiteSpace(previewImage))
                    {
                        PreviewImage = Image;
                        return;
                    }

                    byte[] bytes = Convert.FromBase64String(previewImage);
                    // 创建一个ImageSource对象
                    PreviewImage = ImageSource.FromStream(() => new MemoryStream(bytes));
                    IPreviewImage = PlatformImage.FromStream(new MemoryStream(bytes));
#else
                    PreviewImage = Image;
                    IPreviewImage = PlatformImage.FromStream(new MemoryStream(Convert.FromBase64String(Parameters.Base64)));
#endif
                    OnPropertyChanged(nameof(PreviewImage));
                    OnPropertyChanged(nameof(IPreviewImage));
                    Application.Current!.Dispatcher.Dispatch(() =>
                    {
                        TipsBmpSizeW = $"≈ {(int)(ImageWidth * ((double)Parameters.BmpSizeWPercentage / 100))} / {ImageWidth}";
                        TipsBmpSizeH = $"≈ {(int)(ImageHeight * ((double)Parameters.BmpSizeHPercentage / 100))} / {ImageHeight}";
                        OnPropertyChanged(nameof(TipsBmpSizeW));
                        OnPropertyChanged(nameof(TipsBmpSizeH));
                    });

                    ParametersChanged?.Invoke(Parameters, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"FilePicker Error: {ex.Message}");
                    // The user canceled or something went wrong
                    Application.Current!.Dispatcher.Dispatch(() =>
                    {
                        if (Application.Current is null || Application.Current.MainPage is null) return;
                        _ = Application.Current.MainPage.DisplayAlert("错误", ex.Message, "OK");
                    });
                }
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PrinterImagePreview Error: {ex.Message}");
            // The user canceled or something went wrong
            Application.Current!.Dispatcher.Dispatch(() =>
            {
                if (Application.Current is null || Application.Current.MainPage is null) return;
                _ = Application.Current.MainPage.DisplayAlert("错误", ex.Message, "OK");
            });
        }
        finally
        {
            try
            {
                CancellationTokenSource.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PrinterImagePreview Error: {ex.Message}");
                // The user canceled or something went wrong
                Application.Current!.Dispatcher.Dispatch(() =>
                {
                    if (Application.Current is null || Application.Current.MainPage is null) return;
                    _ = Application.Current.MainPage.DisplayAlert("错误", ex.Message, "OK");
                });
            }
        }
    }
}