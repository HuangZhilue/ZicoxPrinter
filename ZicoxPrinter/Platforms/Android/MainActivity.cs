using Android.App;
using Android.Content.PM;
//using ZicoxPrinter.Services.BluetoothHelper;

namespace ZicoxPrinter;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    //public BluetoothReceiver Receiver { get; set; } = null!;

    //protected override void OnCreate(Bundle? savedInstanceState)
    //{
    //    base.OnCreate(savedInstanceState);

    //    // Register for broadcasts when a device is discovered.
    //    IntentFilter filter = new(BluetoothDevice.ActionFound);
    //    Receiver = new BluetoothReceiver();
    //    RegisterReceiver(Receiver, filter);
    //}

    //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
    //{
    //    if (requestCode == BluetoothScanner.RequestBluetoothPermissionCode)
    //    {
    //        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
    //        {
    //            // 权限已授予，可以继续执行蓝牙相关操作
    //            BluetoothScanner.TryEnableBluetooth();
    //        }
    //        else
    //        {
    //            // 用户拒绝了权限请求，您可以根据情况进行处理
    //        }
    //    }

    //    if (requestCode == BluetoothScanner.RequestLocationPermissionCode)
    //    {
    //        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
    //        {
    //            // 权限已授予，可以继续执行蓝牙相关操作
    //            BluetoothScanner.TryEnableBluetoothLocation();
    //        }
    //        else
    //        {
    //            // 用户拒绝了权限请求，您可以根据情况进行处理
    //        }
    //    }

    //    if (Build.VERSION.SdkInt >= BuildVersionCodes.M && OperatingSystem.IsAndroidVersionAtLeast(23))
    //        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    //}

    //protected override void OnDestroy()
    //{
    //    base.OnDestroy();

    //    // Don't forget to unregister the ACTION_FOUND receiver.
    //    if (Receiver is not null)
    //        UnregisterReceiver(Receiver);
    //}
}

