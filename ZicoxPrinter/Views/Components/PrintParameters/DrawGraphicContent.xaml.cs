using Microsoft.Maui.Graphics.Platform;
using System.Collections.ObjectModel;
using ZicoxPrinter.Services;
using ZicoxPrinter.Services.PrinterSDK;

namespace ZicoxPrinter.Views.Components.PrintParameters;

public partial class DrawGraphicContent : ContentView
{
    public ObservableCollection<DitheringType> DitheringTypes { get; set; } = new(Enum.GetValues(typeof(DitheringType)).Cast<DitheringType>());

    public static readonly BindableProperty ParametersProperty = BindableProperty.Create(nameof(Parameters), typeof(DrawGraphicParameters), typeof(DrawGraphicContent), new DrawGraphicParameters());

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
    public bool IsPreview { get; set; } = false;

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
            string newFile = Path.Combine(CacheService.ImageManagerDiskCacheDirectory, photo.FileName);
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
            PrinterImagePreview();
            OnPropertyChanged(nameof(Image));
            OnPropertyChanged(nameof(ImageHeight));
            OnPropertyChanged(nameof(ImageWidth));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FilePicker Error: {ex.Message}");
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, ex.Message, "OK");
        }
    }

    [RelayCommand]
    public void PrinterImagePreview()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Parameters.Base64) || Image is null || IsPreview) return;
            IsPreview = true;
            PreviewImage = null!;
            OnPropertyChanged(nameof(IsPreview));
            Task.Run(() =>
            {
                try
                {
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
                        Com.Api.MyZpSDK.PrinterCore.DitheringType.ValueOf(Parameters.DitheringType.ToString()),
                        Parameters.Base64);
                    if (string.IsNullOrWhiteSpace(previewImage))
                    {
                        throw new Exception(AppResources.生成预览图失败);
                    }

                    byte[] bytes = Convert.FromBase64String(previewImage);
                    IPreviewImage = PlatformImage.FromStream(new MemoryStream(bytes));
                    #region 生成缩略图
                    // 在页面显示图片之前，先限制图片大小，防止内存溢出，比如将图片等比例缩小到屏幕合适的尺寸
                    byte[] resizedImageBytes = IPreviewImage.CreatePreviewImage(bytes);
                    if (resizedImageBytes.Length == 0) throw new Exception(AppResources.生成缩略图失败);

                    PreviewImage = ImageSource.FromStream(() => new MemoryStream(resizedImageBytes));
                    #endregion
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
                        IsPreview = false;
                        OnPropertyChanged(nameof(IsPreview));
                    });

                    ParametersChanged?.Invoke(Parameters, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Application.Current!.Dispatcher.Dispatch(() =>
                    {
                        PreviewImage = null!;
                        IsPreview = false;
                        OnPropertyChanged(nameof(IsPreview));
                    });
                    Debug.WriteLine($"PrinterImagePreview Error: {ex.Message}");
                    // The user canceled or something went wrong
                    ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, ex.Message, "OK");
                }
            });
        }
        catch (Exception ex)
        {
            PreviewImage = null!;
            IsPreview = false;
            OnPropertyChanged(nameof(IsPreview));
            Debug.WriteLine($"FilePicker Error: {ex.Message}");
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, ex.Message, "OK");
        }
    }
}