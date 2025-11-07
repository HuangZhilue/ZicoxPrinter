using System.Collections.ObjectModel;
using System.Text;
using ZicoxPrinter.Services;

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
        BondedDevices =
        [
            new SampleBluetoothDevice("Test1", "CC3-1234-5678"),
            new SampleBluetoothDevice("ABCD", "CC3-1234-5678"),
            new SampleBluetoothDevice("1a1s", "CC3-1234-5678"),
            new SampleBluetoothDevice("qwsedrfg", "CC3-1234-5678"),
            new SampleBluetoothDevice("Honeywell", "CC3-1234-5678")
        ];
#else
        //GetBondedDevices();
#endif
        Task.Run(() =>
        {
            StringBuilder sb = new();
            sb.AppendLine("! 0 200 200 160 1");
            sb.AppendLine("PAGE-WIDTH 240");
            sb.AppendLine("TEXT 0 2 80 80 HELLO WORLD");
            sb.AppendLine("PRINT");
            CustomCommand = sb.ToString();
            // 无法直接调用MauiAssetService.LoadStringAsset()方法！
            // 可能是因为在注册服务时Asset还未加载，导致安卓实机运行时报错：android.content.res.Resources$NotFoundException
            // 也可能是 MAUI 目前的 BUG。
            // 故目前暂时加载上面写死的文本
        });
    }

    [RelayCommand]
    public void GetBondedDevices()
    {
#if ANDROID
        BondedDevices.Clear();
        if (
            BluetoothScanner.IsBluetoothAvailable()
            != Com.Api.MyBluetoothLibrary.MyCustomResults.Success
        )
        {
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, AppResources.当前设备的蓝牙不可用, "OK");
            return;
        }

        if (
            BluetoothScanner.IsBluetoothEnabled()
            != Com.Api.MyBluetoothLibrary.MyCustomResults.Success
        )
        {
            Task.Run(() =>
            {
                BluetoothScanner.TryEnableBluetooth();
            });
            return;
        }

        // 搜索蓝牙设备
        Dictionary<string, string> devices =
            BluetoothScanner.BondedDevices?.ToDictionary(x => x.Key, x => x.Value) ?? []; //.GetBondedDevices();
        foreach (var device in devices)
        {
            BondedDevices.Add(new(device.Key, device.Value));
        }
#endif
    }

    [RelayCommand]
    public async Task LoadCPCLExampleAsync()
    {
        string action = await ApplicationEx.DisplayActionSheetOnUIThreadAsync(
            AppResources.加载CPCL示例,
            AppResources.取消,
            null,
            [AppResources.HelloWorld, AppResources.CPCL尺子]
        );
        if (string.IsNullOrWhiteSpace(action) || action == AppResources.取消)
            return;
        if (action == AppResources.HelloWorld)
        {
            CustomCommand = await MauiAssetService
                .LoadStringAsset("CPCLCommand_HelloWorld.txt")
                .ConfigureAwait(false);
        }
        else if (action == AppResources.CPCL尺子)
        {
            CustomCommand = await MauiAssetService
                .LoadStringAsset("CPCLCommand_CPCLRules.txt")
                .ConfigureAwait(false);
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

        Task.Run(() =>
        {
            try
            {
#if ANDROID
                IsPrinting = true;
                Services.PrinterSDK.Printer.PrintCommand(
                    BondedDevices[SelectedBondedDeviceIndex].Mac,
                    CustomCommand
                );
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
}
