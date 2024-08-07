﻿using Microsoft.Maui.Graphics.Platform;
using System.Collections.ObjectModel;
using ZicoxPrinter.Services;
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
    [ObservableProperty]
    private bool isPreview = false;

    public ObservableCollection<SampleBluetoothDevice> BondedDevices { get; set; } = [];
    public ObservableCollection<DitheringType> DitheringTypes { get; } = new([.. Enum.GetValues<DitheringType>()]);

    private string ImageBase64 { get; set; } = string.Empty;
    [ObservableProperty]
    private Microsoft.Maui.Graphics.IImage iPreviewImage /*{ get; set; }*/ = null!;

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
        //GetBondedDevices();
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
        if (BluetoothScanner.IsBluetoothAvailable() != Com.Api.MyBluetoothLibrary.MyCustomResults.Success)
        {
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, AppResources.当前设备的蓝牙不可用, "OK");
            return;
        }

        if (BluetoothScanner.IsBluetoothEnabled() != Com.Api.MyBluetoothLibrary.MyCustomResults.Success)
        {
            Task.Run(() =>
            {
                BluetoothScanner.TryEnableBluetooth();
            });
            return;
        }

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
            Microsoft.Maui.Graphics.IImage image = null!;
            string newFile = Path.Combine(CacheService.ImageManagerDiskCacheDirectory, photo.FileName);
            using (Stream stream = await photo.OpenReadAsync())
            using (FileStream newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
                stream.Position = 0;
                image = PlatformImage.FromStream(stream);
                ImageHeight = (int)image.Height;
                ImageWidth = (int)image.Width;
            }

            using MemoryStream memoryStream = new();
            using FileStream fileStream = File.OpenRead(newFile);
            fileStream.CopyTo(memoryStream);
            byte[] bytes = memoryStream.ToArray();

            #region 生成缩略图
            // 在页面显示图片之前，先限制图片大小，防止内存溢出，比如将图片等比例缩小到屏幕合适的尺寸
            byte[] resizedImageBytes = image.CreatePreviewImage(bytes);
            Image = ImageSource.FromStream(() => new MemoryStream(resizedImageBytes));
            #endregion

            ImageBase64 = Convert.ToBase64String(bytes);
            PrinterImagePreview();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FilePicker Error: {ex.Message}");
            // The user canceled or something went wrong
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, ex.Message, "OK");
        }
    }

    [RelayCommand]
    public void PrinterImagePreview()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ImageBase64) || Image is null || IsPreview) return;
            IsPreview = true;
            PreviewImage = null!;
            Task.Run(() =>
            {
                try
                {
                    Debug.WriteLine($"PrinterImagePreview Start At\t{DateTime.Now:HH:mm:ss.fff}");
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
                        Com.Api.MyZpSDK.PrinterCore.DitheringType.ValueOf(DrawBigGraphicParameters.DitheringType.ToString()),
                        ImageBase64);
                    if (string.IsNullOrWhiteSpace(previewImage))
                    {
                        throw new Exception(AppResources.生成预览图失败);
                    }

                    Debug.WriteLine($"PrinterImagePreview Continue At\t{DateTime.Now:HH:mm:ss.fff}");

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
                    IPreviewImage = PlatformImage.FromStream(new MemoryStream(Convert.FromBase64String(ImageBase64)));
#endif
                    Debug.WriteLine($"PrinterImagePreview End At\t{DateTime.Now:HH:mm:ss.fff}");
                    Application.Current!.Dispatcher.Dispatch(() =>
                    {
                        TipsBmpSizeW = $"≈ {(int)(ImageWidth * ((double)DrawBigGraphicParameters.BmpSizeWPercentage / 100))} / {ImageWidth}";
                        TipsBmpSizeH = $"≈ {(int)(ImageHeight * ((double)DrawBigGraphicParameters.BmpSizeHPercentage / 100))} / {ImageHeight}";
                        IsPreview = false;
                    });
                }
                catch (Exception ex)
                {
                    Application.Current!.Dispatcher.Dispatch(() =>
                    {
                        PreviewImage = null!;
                        IsPreview = false;
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
            Debug.WriteLine($"PrinterImagePreview Error: {ex.Message}");
            // The user canceled or something went wrong
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, ex.Message, "OK");
        }
    }

    [RelayCommand]
    public void Print()
    {
        if (BondedDevices == null || BondedDevices.Count == 0)
        {
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, AppResources.没有可用的设备, "OK");
            return;
        }
        if (SelectedBondedDeviceIndex < 0)
        {
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, AppResources.请选择设备, "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(ImageBase64))
        {
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, AppResources.请选择图片, "OK");
            return;
        }

        Task.Run(() =>
        {
            try
            {
                DrawBigGraphicParameters.Base64 = ImageBase64;
                PrintInfo.PrintParameters = [DrawBigGraphicParameters];
                PrintInfo.Address = BondedDevices[SelectedBondedDeviceIndex].Mac;

#if ANDROID
                IsPrinting = true;
                Printer.Print(PrintInfo);
                IsPrinting = false;
#endif
            }
            catch (Exception ex)
            {
                ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, ex.Message, "OK");
                IsPrinting = false;
            }
        });
    }

    [RelayCommand]
    public void AutoFill()
    {
        if (ImageWidth <= 0 || ImageHeight <= 0 || IsPreview) return;
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
        PrinterImagePreview();
    }
}
