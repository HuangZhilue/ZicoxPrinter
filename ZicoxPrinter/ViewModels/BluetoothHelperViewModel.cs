using System.Collections.ObjectModel;
using ZicoxPrinter.Services;

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

        AppShell.ShellNavigated += (sender, e) =>
        {
            if (sender is AppShell shell && shell.CurrentPage.BindingContext.GetType() == typeof(BluetoothHelperViewModel))
            {
                NavigatedTo();
            }
            else
            {
                NavigatedFrom();
            }
        };
    }

    private CancellationTokenSource _cancellationTokenSource = null!;

    [RelayCommand]
    public void NavigatedTo()
    {
        try
        {
            // 取消之前的任务
            _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(2500, cancellationToken).ConfigureAwait(false);

                        if (cancellationToken.IsCancellationRequested) break;

                        CheckBluetoothEnable();
                        Debug.WriteLine($"IsBluetoothEnabled:\t{IsBluetoothEnabled}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex}");
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when OnNavigatedTo:\t{ex}");
        }
    }

    [RelayCommand]
    public void NavigatedFrom()
    {
        try
        {
            // 取消之前的任务
            if (_cancellationTokenSource == null) return;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null!;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when OnNavigatedFrom:\t{ex}");
        }
    }

    [RelayCommand]
    public void CheckBluetoothAvailable()
    {
#if ANDROID
        IsBluetoothAvailable = BluetoothScanner.IsBluetoothAvailable() == Com.Api.MyBluetoothLibrary.MyCustomResults.Success;
        if (IsBluetoothAvailable)
            CheckBluetoothEnable();
#else
        IsBluetoothAvailable = false;
#endif
    }

    [RelayCommand]
    public void CheckBluetoothEnable()
    {
#if ANDROID
        IsBluetoothEnabled = BluetoothScanner.IsBluetoothEnabled() == Com.Api.MyBluetoothLibrary.MyCustomResults.Success;
#else
        IsBluetoothEnabled = false;
#endif
    }

    [RelayCommand]
    public void SwitchBluetoothToggle()
    {
        Debug.WriteLine($"SwitchBluetoothToggle:\t{IsBluetoothEnabled}");

        if (!IsBluetoothEnabled)
        {
#if ANDROID
            BluetoothScanner.TryEnableBluetooth();
#else
            IsBluetoothEnabled = true;
#endif
        }
        else
        {
#if ANDROID
            BluetoothScanner.TryDisableBluetooth();
#else
            IsBluetoothEnabled = false;
#endif
        }
    }

    [RelayCommand]
    public void GetBondedDevices()
    {
        BondedDevices.Clear();
#if ANDROID
        // 搜索蓝牙设备
        Dictionary<string, string> devices = BluetoothScanner.BondedDevices?.ToDictionary(x => x.Key, x => x.Value) ?? [];//.GetBondedDevices();
        foreach (var device in devices)
        {
            BondedDevices.Add(new(device.Key, device.Value));
        }
#else
        BondedDevices.Add(new("test1", "12:34:56:78:90:AB"));
        BondedDevices.Add(new("test2", "12:34:56:78:90:AB"));
        BondedDevices.Add(new("test3", "12:34:56:78:90:AB"));
#endif
    }

    [RelayCommand]
    public void ScanClassicDevices()
    {
        if (IsScanning) return;

        NotBondedDevices.Clear();
        try
        {
#if ANDROID
            BluetoothScanner.UnregisterReceiver();
            BluetoothScanner.RegisterReceiver();
            var result = BluetoothScanner.ScanClassicDevices();
            Debug.WriteLine($"BluetoothScanner.ScanClassicDevices :\t{result}");
            if (result == Com.Api.MyBluetoothLibrary.MyCustomResults.Success)
#else
            if (true)
#endif
            {
                IsScanning = true;
                Task.Run(async () =>
                {
                    DateTime startTime = DateTime.Now;
                    while (startTime.AddSeconds(12) > DateTime.Now)
                    {
#if ANDROID
                        Dictionary<string, string> devices = BluetoothScanner.FoundClassicDevices()?.ToDictionary(x => x.Key, x => x.Value) ?? [];

                        //Application.Current!.Dispatcher.Dispatch(() =>
                        //{
                        foreach (var device in devices)
                        {
                            if (!NotBondedDevices.Any(d => d.Name == device.Key && d.Mac == device.Value))
                                NotBondedDevices.Add(new(device.Key, device.Value));
                        }
                        //});
#else
                        Application.Current!.Dispatcher.Dispatch(() =>
                        {
                            NotBondedDevices.Add(new("test" + DateTime.Now.ToString("ssff"), "12:34:56:78:90:AB"));
                        });
#endif
                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                    IsScanning = false;
#if ANDROID
                    BluetoothScanner.UnregisterReceiver();
#endif
                });
            }
        }
        catch (Exception ex)
        {
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, "Scan Failed!\t" + ex.Message, "OK");
        }
        finally
        {
            //BluetoothScanner.UnregisterReceiver();
        }
    }

    [RelayCommand]
    public void ConnectToDevice(string mac)
    {
#if ANDROID
        try
        {
            BluetoothScanner.Bond(mac);
        }
        catch (Exception ex)
        {
            // 连接失败，处理异常
            ApplicationEx.DisplayAlertOnUIThread(AppResources.错误, "Failed to connect: " + ex.Message, "OK");
        }
#endif
    }
}

public class SampleBluetoothDevice
{
    public string Name { get; set; } = string.Empty;
    public string Mac { get; set; } = string.Empty;
    public string DisplayName => $"{Name} ({Mac})";

    public SampleBluetoothDevice()
    {

    }

    public SampleBluetoothDevice(string name, string mac)
    {
        Name = name;
        Mac = mac;
    }
}
