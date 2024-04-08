#if ANDROID
using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace ZicoxPrinter.Services.BluetoothHelper;

public static class BluetoothScanner
{
    public const int RequestBluetoothPermissionCode = 1;
    public const int RequestLocationPermissionCode = 2;

    private static Dictionary<string, string> BondedDevices { get; } = [];
    private static Dictionary<string, string> NotBondedDevices { get; } = [];
    private static BluetoothAdapter BluetoothAdapter { get; set; } = null!;

    public static bool IsBluetoothAvailable()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr2 && OperatingSystem.IsAndroidVersionAtLeast(18))
        {
            if (Android.App.Application.Context.GetSystemService(Context.BluetoothService) is not BluetoothManager bluetoothManager)
            {
                Toast.MakeText(Platform.AppContext!, "BluetoothManager Is Null", ToastLength.Long)?.Show();
                return false;
            }

            BluetoothAdapter = bluetoothManager.Adapter!;
        }

        if (BluetoothAdapter is null && Build.VERSION.SdkInt < BuildVersionCodes.S && !OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            BluetoothAdapter = BluetoothAdapter.DefaultAdapter!;
        }

        if (BluetoothAdapter is null)
        {
            Toast.MakeText(Platform.AppContext!, "当前设备没有找到蓝牙适配器", ToastLength.Long)?.Show();
            return false;
        }

        return true;
    }

    public static bool IsBluetoothEnabled()
    {
        if (!IsBluetoothAvailable()) return false;
        return BluetoothAdapter.IsEnabled;
    }

    public static bool RequestBluetoothPermission()
    {
        bool permission = true;

        if (Build.VERSION.SdkInt > BuildVersionCodes.R && OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.BluetoothConnect) != Permission.Granted)
            {
                permission = false;
                ActivityCompat.RequestPermissions(Platform.CurrentActivity!, [Manifest.Permission.BluetoothConnect], RequestBluetoothPermissionCode);
            }
            if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.BluetoothScan) != Permission.Granted)
            {
                permission = false;
                ActivityCompat.RequestPermissions(Platform.CurrentActivity!, [Manifest.Permission.BluetoothScan], RequestBluetoothPermissionCode);
            }
        }

        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.Bluetooth) != Permission.Granted)
        {
            permission = false;
            ActivityCompat.RequestPermissions(Platform.CurrentActivity!, [Manifest.Permission.Bluetooth], RequestBluetoothPermissionCode);
        }
        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.BluetoothAdmin) != Permission.Granted)
        {
            permission = false;
            ActivityCompat.RequestPermissions(Platform.CurrentActivity!, [Manifest.Permission.BluetoothAdmin], RequestBluetoothPermissionCode);
        }

        return permission;
    }

    public static bool RequestBluetoothScanPermission()
    {
        bool permission = true;

        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.AccessFineLocation) != Permission.Granted)
        {
            permission = false;
            ActivityCompat.RequestPermissions(Platform.CurrentActivity!, [Manifest.Permission.AccessFineLocation], RequestLocationPermissionCode);
        }
        if (ContextCompat.CheckSelfPermission(Platform.CurrentActivity!, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
        {
            permission = false;
            ActivityCompat.RequestPermissions(Platform.CurrentActivity!, [Manifest.Permission.AccessCoarseLocation], RequestLocationPermissionCode);
        }

        return permission;
    }

    public static bool TryEnableBluetooth()
    {
        if (!RequestBluetoothPermission()) return false;

        if (BluetoothAdapter.IsEnabled) return true;

        if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu && !OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            BluetoothAdapter.Enable();
        }
        else
        {
            Intent enableBtIntent = new(BluetoothAdapter.ActionRequestEnable);
            enableBtIntent.AddFlags(ActivityFlags.NewTask);
            Platform.AppContext.StartActivity(enableBtIntent, new(RequestBluetoothPermissionCode));
        }

        return false;
    }

    public static void TryDisableBluetooth()
    {
        if (!RequestBluetoothPermission()) return;

        if (!BluetoothAdapter.IsEnabled) return;

        if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu && !OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            BluetoothAdapter.Disable();
        }
        else
        {
            Intent enableBtIntent = new("android.bluetooth.adapter.action.REQUEST_DISABLE");
            enableBtIntent.AddFlags(ActivityFlags.NewTask);
            Platform.AppContext.StartActivity(enableBtIntent, new(RequestBluetoothPermissionCode));
        }
    }

    public static bool TryEnableBluetoothLocation()
    {
        if (!RequestBluetoothScanPermission()) return false;
        if (Android.App.Application.Context.GetSystemService(Context.LocationService) is LocationManager locationManager)
        {
            if (!locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                // 如果GPS未启用，启动系统设置页面以允许用户手动启用GPS
                Intent enableGpsIntent = new(Android.Provider.Settings.ActionLocationSourceSettings);
                enableGpsIntent.AddFlags(ActivityFlags.NewTask);
                Platform.AppContext.StartActivity(enableGpsIntent);
                return false;
            }
            return true;
        }
        return false;
    }

    //public static void TryDisableBluetoothLocation()
    //{
    //    if (!CheckBluetoothScanPermission()) return;
    //    if (Android.App.Application.Context.GetSystemService(Context.LocationService) is LocationManager locationManager)
    //    {
    //        if (locationManager.IsProviderEnabled(LocationManager.GpsProvider))
    //        {
    //            locationManager.RemoveUpdates((ILocationListener)Platform.CurrentActivity);
    //        }
    //    }
    //}

    public static Dictionary<string, string> GetBondedDevices()
    {
        BondedDevices.Clear();
        if (!TryEnableBluetooth()) return [];

        List<BluetoothDevice> pairedDevices = [.. BluetoothAdapter.BondedDevices];

        foreach (BluetoothDevice device in pairedDevices)
        {
            if (string.IsNullOrWhiteSpace(device.Name) || string.IsNullOrWhiteSpace(device.Address)) continue;
            BondedDevices.Add(device.Name, device.Address);
        }

        if (BondedDevices.Count == 0)
        {
            Toast.MakeText(Platform.AppContext!, "当前设备没有匹配到蓝牙设备", ToastLength.Long)?.Show();
        }

        return BondedDevices;
    }

    public static Dictionary<string, string> FoundClassicDevices()
    {
        return NotBondedDevices;
    }

    public static bool ScanClassicDevices()
    {
        if (!TryEnableBluetooth()) return false;
        if (!RequestBluetoothScanPermission()) return false;
        if (BluetoothAdapter.IsDiscovering)
            BluetoothAdapter.CancelDiscovery();
        BluetoothReceiver.OnDeviceFound -= OnDeviceFound;
        NotBondedDevices.Clear();
        if (!TryEnableBluetoothLocation()) return false;
        BluetoothReceiver.OnDeviceFound += OnDeviceFound;

        //if (Android.App.Application.Context.GetSystemService(Context.LocationService) is LocationManager locationManager)
        //{
        //    if (!locationManager.IsProviderEnabled(LocationManager.GpsProvider))
        //    {
        //        // 如果GPS未启用，启动系统设置页面以允许用户手动启用GPS
        //        Intent enableGpsIntent = new(Android.Provider.Settings.ActionLocationSourceSettings);
        //        enableGpsIntent.AddFlags(ActivityFlags.NewTask);
        //        Platform.AppContext.StartActivity(enableGpsIntent);
        //    }
        //}

        return BluetoothAdapter.StartDiscovery();
    }

    private static void OnDeviceFound(object? sender, Dictionary<string, string> e)
    {
        if (e.Count > 0)
        {
            foreach (var item in e)
            {
                if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                    NotBondedDevices.TryAdd(item.Key, item.Value);
            }
        }
    }

    public static bool Bond(string address)
    {
        if (!TryEnableBluetooth()) return false;
        if (BluetoothAdapter.IsDiscovering)
            BluetoothAdapter.CancelDiscovery();
        BluetoothDevice? bluetoothDevice = BluetoothAdapter.GetRemoteDevice(address);
        if (bluetoothDevice is null) return false;
        return bluetoothDevice.CreateBond();
    }
}

public class BluetoothReceiver : BroadcastReceiver
{
    public static event EventHandler<Dictionary<string, string>> OnDeviceFound = null!;
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent is null) return;
        string action = intent.Action ?? string.Empty;
        if (BluetoothDevice.ActionFound.Equals(action))
        {
            // Discovery has found a device. Get the BluetoothDevice
            // object and its info from the Intent.
            BluetoothDevice device = null!;
            if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu && !OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                if (intent.GetParcelableExtra(BluetoothDevice.ExtraDevice) is BluetoothDevice device1)
                {
                    device = device1;
                }
                else return;
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu && OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                if (intent.GetParcelableExtra(BluetoothDevice.ExtraDevice, Java.Lang.Class.FromType(typeof(BluetoothDevice))) is BluetoothDevice device1)
                {
                    device = device1;
                }
                else return;
            }
            if (device is null) return;

            string deviceName = device.Name ?? string.Empty;
            string deviceHardwareAddress = device.Address ?? string.Empty; // MAC address
            OnDeviceFound?.Invoke(null, new Dictionary<string, string> { { deviceName, deviceHardwareAddress } });
        }
    }
}
#endif