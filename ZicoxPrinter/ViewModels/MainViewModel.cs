using System.Collections.ObjectModel;
using ZicoxPrinter.Services.PrinterSDK;
#if ANDROID
//using ZicoxPrinter.Services.BluetoothHelper;
using Com.Api.MyBluetoothLibrary;
#endif

namespace ZicoxPrinter.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty]
    private int selectedBondedDeviceIndex = 0;
    [ObservableProperty]
    private ImageSource image = null!;
    [ObservableProperty]
    private DrawBigGraphicParameters drawBigGraphicParameters = new()
    {
        StartX = 0,
        StartY = 0,
        BmpSizeHPercentage = 100,
        BmpSizeWPercentage = 100,
        Rotate = 0,
        Threshold = 128,
        DitheringType = DitheringType.Burkes
    };
    [ObservableProperty]
    private PrintInfo printInfo = new()
    {
        PageHeight = 200,
        PageWidth = 200
    };

    public ObservableCollection<SampleBluetoothDevice> BondedDevices { get; set; } = [];
    public ObservableCollection<DitheringType> DitheringTypes { get; } = new([.. Enum.GetValues<DitheringType>()]);

    private string ImageBase64 { get; set; } = string.Empty;
#if ANDROID
    private MyBluetoothHelper BluetoothScanner { get; set; } = new(Platform.AppContext, Platform.CurrentActivity);
#endif

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
#endif
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
            }

            Image = ImageSource.FromFile(newFile);

            using MemoryStream memoryStream = new();
            using FileStream fileStream = File.OpenRead(newFile);
            fileStream.CopyTo(memoryStream);
            byte[] bytes = memoryStream.ToArray();
            ImageBase64 = Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FilePicker Error: {ex.Message}");
            // The user canceled or something went wrong
            if (Application.Current is null || Application.Current.MainPage is null) return;
            _ = Application.Current.MainPage.DisplayAlert("错误", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public void Print()
    {
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
            Printer.Print(PrintInfo);
#endif
        }
        catch (Exception ex)
        {
            _ = Application.Current!.MainPage!.DisplayAlert("错误", ex.Message, "OK");
        }
    }
}
