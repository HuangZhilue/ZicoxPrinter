﻿#if ANDROID
//using Com.Api.MyBluetoothLibrary;
#endif
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

        builder.Services.AddSingleton<BluetoothHelperViewModel>();

        builder.Services.AddSingleton<BluetoothHelperPage>();

        //#if ANDROID
        //        MyBluetoothHelper bluetoothScanner = new(Platform.AppContext, Platform.CurrentActivity);
        //        builder.Services.AddSingleton(bluetoothScanner);
        //#endif

        return builder.Build();
    }
}