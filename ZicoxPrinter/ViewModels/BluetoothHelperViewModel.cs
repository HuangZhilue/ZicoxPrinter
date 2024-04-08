using System.Collections.ObjectModel;

#if ANDROID
using ZicoxPrinter.Services.BluetoothHelper;
#endif

namespace ZicoxPrinter.ViewModels;

public partial class BluetoothHelperViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool isBluetoothAvailable;
    [ObservableProperty]
    private bool isBluetoothEnabled;
    [ObservableProperty]
    private bool isScanning;
    public ObservableCollection<SampleBluetoothDevice> BondedDevices { get; set; } = [];
    public ObservableCollection<SampleBluetoothDevice> NotBondedDevices { get; set; } = [];

    public BluetoothHelperViewModel()
    {
        CheckBluetoothAvailable();
        if (IsBluetoothAvailable)
        {
            CheckBluetoothEnable();
        }
    }

    [RelayCommand]
    public void CheckBluetoothAvailable()
    {
#if ANDROID
        IsBluetoothAvailable = BluetoothScanner.IsBluetoothAvailable();
        if (IsBluetoothAvailable)
            CheckBluetoothEnable();
#endif
    }

    [RelayCommand]
    public void CheckBluetoothEnable()
    {
#if ANDROID
        IsBluetoothEnabled = BluetoothScanner.IsBluetoothEnabled();
#endif
    }

    [RelayCommand]
    public void TryEnableBluetooth()
    {
#if ANDROID
        BluetoothScanner.TryEnableBluetooth();
        Task.Run(async () =>
        {
            await Task.Delay(1000).ConfigureAwait(false);
            CheckBluetoothEnable();
        });
#endif
    }

    [RelayCommand]
    public void TryDisableBluetooth()
    {
#if ANDROID
        BluetoothScanner.TryDisableBluetooth();
        Task.Run(async () =>
        {
            await Task.Delay(1000).ConfigureAwait(false);
            CheckBluetoothEnable();
        });
#endif
    }

    [RelayCommand]
    public void GetBondedDevices()
    {
#if ANDROID
        BondedDevices.Clear();

        // 搜索蓝牙设备
        Dictionary<string, string> devices = BluetoothScanner.GetBondedDevices();
        foreach (var device in devices)
        {
            BondedDevices.Add(new(device.Key, device.Value));
        }
#endif
    }

    [RelayCommand]
    public void ScanClassicDevices()
    {
#if ANDROID
        if (BluetoothScanner.ScanClassicDevices())
        {
            IsScanning = true;
            NotBondedDevices.Clear();
            Task.Run(async () =>
            {
                DateTime startTime = DateTime.Now;
                while (startTime.AddSeconds(12) > DateTime.Now)
                {
                    Dictionary<string, string> devices = BluetoothScanner.FoundClassicDevices();
                    foreach (var device in devices)
                    {
                        if (!NotBondedDevices.Any(d => d.Name == device.Key && d.Mac == device.Value))
                            NotBondedDevices.Add(new(device.Key, device.Value));
                    }
                    await Task.Delay(1000).ConfigureAwait(false);
                }
                IsScanning = false;
            });
        }
#endif
    }

    [RelayCommand]
    public static async Task ConnectToDeviceAsync(string mac)
    {
#if ANDROID
        try
        {
            BluetoothScanner.Bond(mac);
        }
        catch (Exception ex)
        {
            // 连接失败，处理异常
            await Application.Current!.MainPage!.DisplayAlert("Error", "Failed to connect: " + ex.Message, "OK");
        }
#endif
    }
}

public class SampleBluetoothDevice
{
    public string Name { get; set; } = string.Empty;
    public string Mac { get; set; } = string.Empty;

    public SampleBluetoothDevice()
    {

    }

    public SampleBluetoothDevice(string name, string mac)
    {
        Name = name;
        Mac = mac;
    }
}
