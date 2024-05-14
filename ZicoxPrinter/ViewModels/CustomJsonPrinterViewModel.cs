using Newtonsoft.Json;
using System.Collections.ObjectModel;
using ZicoxPrinter.Services;
using ZicoxPrinter.Services.PrinterSDK;

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
    //[ObservableProperty]
    //private string customPrintParametersJsonString = string.Empty;
    [ObservableProperty]
    private string lastPrintData = string.Empty;
    [ObservableProperty]
    private bool isPrinting = false;

    public ObservableCollection<SampleBluetoothDevice> BondedDevices { get; set; } = [];

    public ObservableCollection<PrintParametersBase> CustomPrintParameters { get; set; } = [];
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
        //GetBondedDevices();
#endif

        CustomPrintParameters = [
            //new DrawBoxParameters() { LineWidth = 1, TopLeftX = 10, TopLeftY = 10, BottomRightX = PrintInfo.PageWidth - 10, BottomRightY = PrintInfo.PageHeight - 10 },
            new DrawText1Parameters() { Text = nameof(ZicoxPrinter), FontSize = TextFont.EN1, TextX = 20, TextY = 20, Reverse = false, Rotate = 0, Underline = false, Bold = false },

            //new DrawBoxParameters(){ LineWidth = 1, TopLeftX = 10, TopLeftY = 10, BottomRightX = 566, BottomRightY = 290 },
            //new DrawLineParameters(){ StartX = 10, StartY = 10, EndX = 566, EndY = 290, LineWidth = 1 },
            //new DrawText2Parameters(){ Text = "this is DrawText2,with height and width", TextX = 20, TextY = 120,Width=250,Height=200,FontSize= TextFont.EN1},
            //new DrawBarCodeParameters(){StartX=160,StartY=180,Type = BarcodeType.Codabar, Text = "20240511", LineWidth = 2, Height = 80},
            //new DrawQrCodeParameters(){StartX=80,StartY=180,Ver=3,Text="20240511"},
            //new DrawGraphicParameters(){StartX = 400, StartY=120,BmpSizeWPercentage=50,BmpSizeHPercentage=50},

            //new DrawLineParameters() { LineWidth = 1, StartX = 15, StartY = 15, EndX = PrintInfo.PageWidth - 15, EndY = PrintInfo.PageHeight - 15 },
            //new DrawText2Parameters() { Text = nameof(DrawText2Parameters), FontSize = TextFont.EN1, TextX = 120, TextY = 120, Reverse = false, Rotate = 0, Underline = false, Bold = false , Width = PrintInfo.PageWidth - 40, Height = PrintInfo.PageHeight - 40 },
            //new DrawBarCodeParameters() { Text = DateTime.Now.ToString("yyyyMMddHHmmss"), StartX = 10, StartY = 40, Height = 25, LineWidth = 2, Rotate = false, Type = BarcodeType.Code39 },
            //new DrawQrCodeParameters() { Text = DateTime.Now.ToString("yyyyMMddHHmmss"), StartX = 10, StartY = 70, Ver = 8 },
            //new DrawGraphicParameters() {}
        ];
        //CustomPrintParametersJsonString = JsonConvert.SerializeObject(CustomPrintParameters, Formatting.Indented, JsonSerializerSettings);
    }

    [RelayCommand]
    public void GetBondedDevices()
    {
#if ANDROID
        BondedDevices.Clear();
        if (!BluetoothScanner.IsBluetoothAvailable)
        {
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, AppResources.当前设备的蓝牙不可用, "OK");
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
                //CustomPrintParameters = JsonConvert.DeserializeObject<ObservableCollection<PrintParametersBase>>(CustomPrintParametersJsonString, JsonSerializerSettings) ?? [];
                if (CustomPrintParameters.Count == 0)
                {
                    ApplicationEx.DisplayAlertOnUIThread(AppResources.提示, AppResources.没有有效的打印参数, "OK");
                    return;
                }
                PrintInfo.PrintParameters = [.. CustomPrintParameters];
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
    public void PrintCPCLRuler()
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
                PrintInfo.Address = BondedDevices[SelectedBondedDeviceIndex].Mac;
                Printer.PrintCPCLRuler(PrintInfo.Address, PrintInfo.PageWidth, PrintInfo.PageHeight);
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
