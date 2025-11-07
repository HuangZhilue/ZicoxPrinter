#if ANDROID
using Com.Api.MyBluetoothLibrary;
#endif

namespace ZicoxPrinter.ViewModels;

public partial class BaseViewModel : ObservableObject
{
#if ANDROID
    private MyBluetoothHelper bluetoothScanner = null!;

    protected MyBluetoothHelper BluetoothScanner
    {
        get
        {
            try
            {
                bluetoothScanner ??=
                    MyBluetoothHelper.Instance
                    ?? MyBluetoothHelper.Init(Platform.AppContext, Platform.CurrentActivity)
                    ?? throw new Exception(AppResources.初始化蓝牙模块失败);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                try
                {
                    bluetoothScanner ??=
                        MyBluetoothHelper.Init(Platform.AppContext, Platform.CurrentActivity)
                        ?? throw new Exception(AppResources.初始化蓝牙模块失败);
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine(ex2);
                }
            }
            return bluetoothScanner;
        }
    }
#endif

    public BaseViewModel()
    {
        if (Application.Current is not null)
            App.SetStatusBar(Application.Current.UserAppTheme);
    }
}
