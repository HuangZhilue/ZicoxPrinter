using UraniumUI;
using ZicoxPrinter.Views;

namespace ZicoxPrinter;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("FontAwesome6FreeBrands.otf", "FontAwesomeBrands");
                fonts.AddFont("FontAwesome6FreeRegular.otf", "FontAwesomeRegular");
                fonts.AddFont("FontAwesome6FreeSolid.otf", "FontAwesomeSolid");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddMaterialIconFonts();
            });

        builder.Services.AddSingleton<MainViewModel>();

        builder.Services.AddSingleton<MainPage>();

        builder.Services.AddSingleton<CustomJsonPrinterViewModel>();

        builder.Services.AddSingleton<CustomJsonPrinterPage>();

        builder.Services.AddSingleton<CustomCommandViewModel>();

        builder.Services.AddSingleton<CustomCommandPage>();

        builder.Services.AddSingleton<BluetoothHelperViewModel>();

        builder.Services.AddSingleton<BluetoothHelperPage>();

        builder.Services.AddSingleton<SettingsViewModel>();

        builder.Services.AddSingleton<SettingsPage>();

        //#if ANDROID
        //        MyBluetoothHelper bluetoothScanner = new(Platform.AppContext, Platform.CurrentActivity);
        //        builder.Services.AddSingleton(bluetoothScanner);
        //#endif

#if DEBUG
        //CultureInfo.CurrentCulture = new CultureInfo("en", false);
        //CultureInfo.CurrentUICulture = new CultureInfo("en", false);
#endif

        return builder.Build();
    }
}
