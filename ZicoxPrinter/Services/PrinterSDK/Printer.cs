#if ANDROID

namespace ZicoxPrinter.Services.PrinterSDK;

public class Printer
{
    private static Com.Api.MyZpSDK.Printer ZpSDK { get; } = Com.Api.MyZpSDK.Printer.InitInstance(Platform.AppContext)!;

    private static void CheckPrintStatus(int status)
    {
        if (status == 0) return;

        Application.Current!.Dispatcher.Dispatch(() =>
        {
            if (status == -2)
            {
                _ = Application.Current.MainPage!.DisplayAlert(AppResources.错误, AppResources.SDK初始化失败, "OK");
            }
            else if (status == -1)
            {
                _ = Application.Current.MainPage!.DisplayAlert(AppResources.错误, AppResources.蓝牙打印机连接失败, "OK");
            }
            else if (status == 1)
            {
                _ = Application.Current.MainPage!.DisplayAlert(AppResources.错误, AppResources.无法打印打印机缺纸, "OK");
            }
            else if (status == 2)
            {
                _ = Application.Current.MainPage!.DisplayAlert(AppResources.错误, AppResources.无法打印打印机开盖, "OK");
            }
            else
            {
                _ = Application.Current.MainPage!.DisplayAlert(AppResources.错误, AppResources.打印失败, "OK");
            }
        });
    }

    public static bool PrintCPCLRuler(string address, int width, int height)
    {
        try
        {
            int result = ZpSDK.PrintCPCLRuler(address, width, height);
            CheckPrintStatus(result);
            return result == 0;
        }
        catch (Exception ex)
        {
            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Application.Current.MainPage!.DisplayAlert(AppResources.错误, ex.Message, "OK");
            });
            return false;
        }
    }

    public static bool Print(PrintInfo info)
    {
        //实例化
        try
        {
            Com.Api.MyZpSDK.Printer.PrintInfo printInfo = new(ZpSDK)
            {
                Address = info.Address,
                PageWidth = info.PageWidth,
                PageHeight = info.PageHeight,
                PrintParameters = info.PrintParameters.Select(p =>
                {
                    Com.Api.MyZpSDK.Printer.PrintParametersBase p2 = null!;
                    switch (p.DrawType)
                    {
                        case DrawType.DrawBox:
                            {
                                DrawBoxParameters p1 = (DrawBoxParameters)p;
                                p2 = new Com.Api.MyZpSDK.Printer.DrawBoxParameters(ZpSDK)
                                {
                                    TopLeftX = p1.TopLeftX,
                                    TopLeftY = p1.TopLeftY,
                                    BottomRightX = p1.BottomRightX,
                                    BottomRightY = p1.BottomRightY,
                                    LineWidth = p1.LineWidth,
                                };
                                break;
                            }
                        case DrawType.DrawLine:
                            {
                                DrawLineParameters p1 = (DrawLineParameters)p;
                                p2 = new Com.Api.MyZpSDK.Printer.DrawLineParameters(ZpSDK)
                                {
                                    LineWidth = p1.LineWidth,
                                    StartX = p1.StartX,
                                    StartY = p1.StartY,
                                    EndX = p1.EndX,
                                    EndY = p1.EndY
                                };
                                break;
                            }
                        case DrawType.DrawText1:
                            {
                                DrawText1Parameters p1 = (DrawText1Parameters)p;
                                p2 = new Com.Api.MyZpSDK.Printer.DrawText1Parameters(ZpSDK)
                                {
                                    Text = p1.Text,
                                    TextX = p1.TextX,
                                    TextY = p1.TextY,
                                    FontSize = Com.Api.MyZpSDK.PrinterCore.TextFont.Values()![(int)p1.FontSize],
                                    Rotate = Com.Api.MyZpSDK.PrinterCore.TextRotate.Values()![(int)p1.Rotate],
                                    Bold = p1.Bold,
                                    Reverse = p1.Reverse,
                                    Underline = p1.Underline,
                                };
                                break;
                            }
                        case DrawType.DrawText2:
                            {
                                DrawText2Parameters p1 = (DrawText2Parameters)p;
                                p2 = new Com.Api.MyZpSDK.Printer.DrawText2Parameters(ZpSDK)
                                {
                                    Text = p1.Text,
                                    TextX = p1.TextX,
                                    TextY = p1.TextY,
                                    Height = p1.Height,
                                    Width = p1.Width,
                                    FontSize = Com.Api.MyZpSDK.PrinterCore.TextFont.Values()![(int)p1.FontSize],
                                    Rotate = Com.Api.MyZpSDK.PrinterCore.TextRotate.Values()![(int)p1.Rotate],
                                    Bold = p1.Bold,
                                    Reverse = p1.Reverse,
                                    Underline = p1.Underline,
                                };
                                break;
                            }
                        case DrawType.DrawBarCode:
                            {
                                DrawBarCodeParameters p1 = (DrawBarCodeParameters)p;
                                p2 = new Com.Api.MyZpSDK.Printer.DrawBarCodeParameters(ZpSDK)
                                {
                                    StartX = p1.StartX,
                                    StartY = p1.StartY,
                                    Text = p1.Text,
                                    Type = Com.Api.MyZpSDK.PrinterCore.BarCodeType.Values()![(int)p1.Type],
                                    Height = p1.Height,
                                    LineWidth = p1.LineWidth,
                                    Rotate = p1.Rotate,
                                };
                                break;
                            }
                        case DrawType.DrawQrCode:
                            {
                                DrawQrCodeParameters p1 = (DrawQrCodeParameters)p;
                                p2 = new Com.Api.MyZpSDK.Printer.DrawQrCodeParameters(ZpSDK)
                                {
                                    StartX = p1.StartX,
                                    StartY = p1.StartY,
                                    Text = p1.Text,
                                    Ver = p1.Ver,
                                };
                                break;
                            }
                        case DrawType.DrawGraphic:
                            {
                                DrawGraphicParameters p1 = (DrawGraphicParameters)p;
                                p2 = new Com.Api.MyZpSDK.Printer.DrawGraphicParameters(ZpSDK)
                                {
                                    WidthLimit = p1.WidthLimit,
                                    HeightLimit = p1.HeightLimit,
                                    Base64 = p1.Base64,
                                    BmpSizeHPercentage = p1.BmpSizeHPercentage,
                                    BmpSizeWPercentage = p1.BmpSizeWPercentage,
                                    DitheringType = Com.Api.MyZpSDK.PrinterCore.DitheringType.Values()![(int)p1.DitheringType],
                                    Rotate = p1.Rotate,
                                    StartX = p1.StartX,
                                    StartY = p1.StartY,
                                    Threshold = p1.Threshold,
                                };
                                break;
                            }
                        case DrawType.DrawBigGraphic:
                            {
                                DrawBigGraphicParameters p1 = (DrawBigGraphicParameters)p;
                                p2 = new Com.Api.MyZpSDK.Printer.DrawBigGraphicParameters(ZpSDK)
                                {
                                    WidthLimit = p1.WidthLimit,
                                    HeightLimit = p1.HeightLimit,
                                    Base64 = p1.Base64,
                                    BmpSizeHPercentage = p1.BmpSizeHPercentage,
                                    BmpSizeWPercentage = p1.BmpSizeWPercentage,
                                    DitheringType = Com.Api.MyZpSDK.PrinterCore.DitheringType.Values()![(int)p1.DitheringType],
                                    Rotate = p1.Rotate,
                                    StartX = p1.StartX,
                                    StartY = p1.StartY,
                                    Threshold = p1.Threshold,
                                };
                                break;
                            }
                    }
                    return p2;
                }).ToList()
            };
            int result = ZpSDK.Print(printInfo);
            CheckPrintStatus(result);
            return result == 0;
        }
        catch (Exception ex)
        {
            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Application.Current.MainPage!.DisplayAlert(AppResources.错误, ex.Message, "OK");
            });
            return false;
        }
    }

    public static bool PrintCommand(string address, string command)
    {
        try
        {
            int result = ZpSDK.SendCommand(address, command);
            CheckPrintStatus(result);
            return result == 0;
        }
        catch (Exception ex)
        {
            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Application.Current.MainPage!.DisplayAlert(AppResources.错误, ex.Message, "OK");
            });
            return false;
        }
    }

    public static bool PrintCommand(string address, byte[] command)
    {
        try
        {
            int result = ZpSDK.SendCommand(address, command);
            CheckPrintStatus(result);
            return result == 0;
        }
        catch (Exception ex)
        {
            Application.Current!.Dispatcher.Dispatch(() =>
            {
                _ = Application.Current.MainPage!.DisplayAlert(AppResources.错误, ex.Message, "OK");
            });
            return false;
        }
    }
}
#endif