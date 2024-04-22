using ZicoxPrinter.Services.PrinterSDK;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace ZicoxPrinter.ViewModels;

public partial class CustomJsonPrinterViewModel : BaseViewModel
{
    [ObservableProperty]
    private int selectedBondedDeviceIndex = 0;
    [ObservableProperty]
    private PrintInfo printInfo = new()
    {
        PageHeight = 300,
        PageWidth = 576
    };
    [ObservableProperty]
    private string customPrintParametersJsonString = string.Empty;
    [ObservableProperty]
    private string lastPrintData = string.Empty;
    [ObservableProperty]
    private bool isPrinting = false;

    public ObservableCollection<SampleBluetoothDevice> BondedDevices { get; set; } = [];

    private List<PrintParametersBase> CustomPrintParameters { get; set; } = [];
    private JsonSerializerSettings JsonSerializerSettings { get; } = new() { TypeNameHandling = TypeNameHandling.All };

    public CustomJsonPrinterViewModel()
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

        CustomPrintParameters = [
            new DrawBoxParameters() { LineWidth = 1, TopLeftX = 10, TopLeftY = 10, BottomRightX = PrintInfo.PageWidth - 10, BottomRightY = PrintInfo.PageHeight - 10 },
            new DrawText1Parameters() { Text = nameof(ZicoxPrinter), FontSize = TextFont.EN1, TextX = 20, TextY = 20, Reverse = false, Rotate = 0, Underline = false, Bold = false },
            new DrawLineParameters() { LineWidth = 1, StartX = 15, StartY = 15, EndX = PrintInfo.PageWidth - 15, EndY = PrintInfo.PageHeight - 15 },
            new DrawText2Parameters() { Text = nameof(DrawText2Parameters), FontSize = TextFont.EN1, TextX = 120, TextY = 120, Reverse = false, Rotate = 0, Underline = false, Bold = false , Width = PrintInfo.PageWidth - 40, Height = PrintInfo.PageHeight - 40 },
            new DrawBarCodeParameters() { Text = DateTime.Now.ToString("yyyyMMddHHmmss"), StartX = 10, StartY = 40, Height = 25, LineWidth = 2, Rotate = false, Type = BarcodeType.Code39 },
            new DrawQrCodeParameters() { Text = DateTime.Now.ToString("yyyyMMddHHmmss"), StartX = 10, StartY = 70, Ver = 8 }
        ];
        CustomPrintParametersJsonString = JsonConvert.SerializeObject(CustomPrintParameters, Formatting.Indented, JsonSerializerSettings);
    }

    [RelayCommand]
    public void GetBondedDevices()
    {
#if ANDROID
        BondedDevices.Clear();

        // 搜索蓝牙设备
        Dictionary<string, string> devices = BluetoothScanner.BondedDevices?.ToDictionary(x => x.Key, x => x.Value) ?? [];//.GetBondedDevices();
        foreach (var device in devices)
        {
            BondedDevices.Add(new(device.Key, device.Value));
        }
#endif
    }

    [RelayCommand]
    public void Print()
    {
        if (SelectedBondedDeviceIndex < 0)
        {
            _ = Application.Current!.MainPage!.DisplayAlert("错误", "请选择设备", "OK");
            return;
        }

        try
        {
            CustomPrintParameters = JsonConvert.DeserializeObject<List<PrintParametersBase>>(CustomPrintParametersJsonString, JsonSerializerSettings) ?? [];
            if (CustomPrintParameters.Count == 0)
            {
                _ = Application.Current!.MainPage!.DisplayAlert("提示", "没有有效的打印参数", "OK");
                return;
            }
            PrintInfo.PrintParameters = [.. CustomPrintParameters];
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
    public void PrintCPCLRuler()
    {
        if (SelectedBondedDeviceIndex < 0)
        {
            _ = Application.Current!.MainPage!.DisplayAlert("错误", "请选择设备", "OK");
            return;
        }

        try
        {
#if ANDROID
            Task.Run(() =>
            {
                IsPrinting = true;
                PrintInfo.Address = BondedDevices[SelectedBondedDeviceIndex].Mac;
                Printer.PrintCPCLRuler(PrintInfo.Address, PrintInfo.PageWidth, PrintInfo.PageHeight);
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
}
