using System.Collections.ObjectModel;
using System.Text;
using ZicoxPrinter.Services;
using ZicoxPrinter.Services.PrinterSDK;

namespace ZicoxPrinter.ViewModels;

public partial class CustomCommandViewModel : BaseViewModel
{
    [ObservableProperty]
    private int selectedBondedDeviceIndex = 0;
    [ObservableProperty]
    private bool isPrinting = false;
    [ObservableProperty]
    private string customCommand = string.Empty;

    public ObservableCollection<SampleBluetoothDevice> BondedDevices { get; set; } = [];

    public CustomCommandViewModel()
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
        Task.Run(async () =>
        {
            CustomCommand = await MauiAssetService.LoadStringAsset("CPCLCommand_HelloWorld.txt").ConfigureAwait(false);
        });
    }

    [RelayCommand]
    public void GetBondedDevices()
    {
#if ANDROID
        BondedDevices.Clear();
        if (!BluetoothScanner.IsBluetoothAvailable)
        {
            _ = Application.Current!.MainPage!.DisplayAlert(AppResources.错误, AppResources.当前设备的蓝牙不可用, "OK");
            return;
        }

        if (!BluetoothScanner.IsBluetoothEnabled)
        {
            Task.Run(() =>
            {
                BluetoothScanner.TryEnableBluetooth();
            });
            return;
        }

        // 搜索蓝牙设备
        Dictionary<string, string> devices = BluetoothScanner.BondedDevices?.ToDictionary(x => x.Key, x => x.Value) ?? [];//.GetBondedDevices();
        foreach (var device in devices)
        {
            BondedDevices.Add(new(device.Key, device.Value));
        }
#endif
    }

    [RelayCommand]
    public async Task LoadCPCLExampleAsync()
    {
        string action = await Application.Current!.MainPage!.DisplayActionSheet(AppResources.加载CPCL示例, AppResources.取消, null, [AppResources.HelloWorld, AppResources.CPCL尺子]);
        if (string.IsNullOrWhiteSpace(action) || action == AppResources.取消) return;
        if (action == AppResources.HelloWorld)
        {
            CustomCommand = await MauiAssetService.LoadStringAsset("CPCLCommand_HelloWorld.txt").ConfigureAwait(false);
        }
        else if (action == AppResources.CPCL尺子)
        {
            CustomCommand = await MauiAssetService.LoadStringAsset("CPCLCommand_CPCLRules.txt").ConfigureAwait(false);
        }
    }

    [RelayCommand]
    public void Print()
    {
        if (BondedDevices == null || BondedDevices.Count == 0)
        {
            _ = Application.Current!.MainPage!.DisplayAlert(AppResources.错误, AppResources.没有可用的设备, "OK");
            return;
        }
        if (SelectedBondedDeviceIndex < 0)
        {
            _ = Application.Current!.MainPage!.DisplayAlert(AppResources.错误, AppResources.请选择设备, "OK");
            return;
        }

        Task.Run(() =>
        {
            try
            {
#if ANDROID
                IsPrinting = true;
                Printer.PrintCommand(BondedDevices[SelectedBondedDeviceIndex].Mac, CustomCommand);
                IsPrinting = false;
#endif
            }
            catch (Exception ex)
            {
                Application.Current!.Dispatcher.Dispatch(() =>
                {
                    _ = Application.Current!.MainPage!.DisplayAlert(AppResources.错误, ex.Message, "OK");
                });
                IsPrinting = false;
            }
        });
    }
}
