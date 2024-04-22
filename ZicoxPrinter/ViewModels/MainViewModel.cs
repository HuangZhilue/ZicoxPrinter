using Microsoft.Maui.Graphics.Platform;
using System.Collections.ObjectModel;
using ZicoxPrinter.Services.PrinterSDK;

namespace ZicoxPrinter.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty]
    private int selectedBondedDeviceIndex = 0;
    [ObservableProperty]
    private ImageSource image = null!;
    [ObservableProperty]
    private ImageSource previewImage = null!;
    [ObservableProperty]
    private int imageHeight = 0;
    [ObservableProperty]
    private int imageWidth = 0;
    [ObservableProperty]
    private DrawBigGraphicParameters drawBigGraphicParameters = new()
    {
        StartX = 0,
        StartY = 0,
        BmpSizeHPercentage = 100,
        BmpSizeWPercentage = 100,
        Rotate = 0,
        Threshold = 128,
        DitheringType = DitheringType.Sierra
    };
    [ObservableProperty]
    private PrintInfo printInfo = new()
    {
        PageHeight = 300,
        PageWidth = 576
    };
    [ObservableProperty]
    private string tipsBmpSizeW = string.Empty;
    [ObservableProperty]
    private string tipsBmpSizeH = string.Empty;
    [ObservableProperty]
    private bool isPrinting = false;

    public ObservableCollection<SampleBluetoothDevice> BondedDevices { get; set; } = [];
    public ObservableCollection<DitheringType> DitheringTypes { get; } = new([.. Enum.GetValues<DitheringType>()]);

    private string ImageBase64 { get; set; } = string.Empty;
    [ObservableProperty]
    public Microsoft.Maui.Graphics.IImage iPreviewImage /*{ get; set; }*/ = null!;

    public MainViewModel()
    {
#if !ANDROID
        BondedDevices = [
            new SampleBluetoothDevice("Test1", "CC3-1234-5678"),
            new SampleBluetoothDevice("ABCD", "CC3-1234-5678"),
            new SampleBluetoothDevice("1a1s", "CC3-1234-5678"),
            new SampleBluetoothDevice("qwsedrfg", "CC3-1234-5678"),
            new SampleBluetoothDevice("Honeywell", "CC3-1234-5678")
            ];
#else
        GetBondedDevices();
#endif
    }

    [RelayCommand]
    public void WhenViewModelReload()
    {
        ImageSource image2 = Image;
        ImageSource previewImage2 = PreviewImage;
        Image = null!;
        PreviewImage = null!;
        Image = image2;
        PreviewImage = previewImage2;
    }

    [RelayCommand]
    public void GetBondedDevices()
    {
#if ANDROID
        BondedDevices.Clear();

        // 搜索蓝牙设备
        Dictionary<string, string> devices = BluetoothScanner.BondedDevices?.ToDictionary(x => x.Key, x => x.Value) ?? [];
        foreach (var device in devices)
        {
            BondedDevices.Add(new(device.Key, device.Value));
        }
#endif
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
            ImageBase64 = Convert.ToBase64String(bytes);
            await PrinterImagePreview().ConfigureAwait(false);
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
#if ANDROID
                    string? previewImage = Com.Api.MyZpSDK.PrinterCore.ProcessImageToBase64(
                        PrintInfo.PageWidth,
                        PrintInfo.PageHeight,
                        DrawBigGraphicParameters.StartX,
                        DrawBigGraphicParameters.StartY,
                        DrawBigGraphicParameters.BmpSizeWPercentage,
                        DrawBigGraphicParameters.BmpSizeHPercentage,
                        DrawBigGraphicParameters.Rotate,
                        DrawBigGraphicParameters.Threshold,
                        Com.Api.MyZpSDK.PrinterCore.DitheringType.Values()![(int)DrawBigGraphicParameters.DitheringType],
                        ImageBase64);
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
                    IPreviewImage = PlatformImage.FromStream(new MemoryStream(Convert.FromBase64String(ImageBase64)));
#endif
                    Application.Current!.Dispatcher.Dispatch(() =>
                    {
                        TipsBmpSizeW = $"≈ {(int)(ImageWidth * ((double)DrawBigGraphicParameters.BmpSizeWPercentage / 100))} / {ImageWidth}";
                        TipsBmpSizeH = $"≈ {(int)(ImageHeight * ((double)DrawBigGraphicParameters.BmpSizeHPercentage / 100))} / {ImageHeight}";
                    });
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

    [RelayCommand]
    public void Print()
    {
        if (BondedDevices == null || BondedDevices.Count == 0)
        {
            _ = Application.Current!.MainPage!.DisplayAlert("错误", "没有可用的设备", "OK");
            return;
        }
        if (SelectedBondedDeviceIndex < 0)
        {
            _ = Application.Current!.MainPage!.DisplayAlert("错误", "请选择设备", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(ImageBase64))
        {
            _ = Application.Current!.MainPage!.DisplayAlert("错误", "请选择图片", "OK");
            return;
        }

        try
        {
            DrawBigGraphicParameters.Base64 = ImageBase64;
            PrintInfo.PrintParameters = [DrawBigGraphicParameters];
            PrintInfo.Address = BondedDevices[SelectedBondedDeviceIndex].Mac;

#if ANDROID
            Task.Run(() =>
            {
                IsPrinting = true;
                Printer.Print(PrintInfo);
                IsPrinting = false;
            });
#endif
        }
        catch (Exception ex)
        {
            _ = Application.Current!.MainPage!.DisplayAlert("错误", ex.Message, "OK");
            IsPrinting = false;
        }
    }

    [RelayCommand]
    public void AutoFill()
    {
        if (ImageWidth <= 0 || ImageHeight <= 0) return;
        DrawBigGraphicParameters.BmpSizeWPercentage = (int)(PrintInfo.PageWidth / (double)ImageWidth * 100);
        if (PrintInfo.PageHeight <= 0)
        {
            DrawBigGraphicParameters.BmpSizeHPercentage = DrawBigGraphicParameters.BmpSizeWPercentage;
        }
        else
        {
            DrawBigGraphicParameters.BmpSizeHPercentage = (int)(PrintInfo.PageHeight / (double)ImageHeight * 100);
        }

        OnPropertyChanged(nameof(DrawBigGraphicParameters));
    }
}
