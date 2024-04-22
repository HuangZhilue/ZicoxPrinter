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
                _ = Application.Current.MainPage!.DisplayAlert("错误", "SDK初始化失败", "OK");
            }
            else if (status == -1)
            {
                _ = Application.Current.MainPage!.DisplayAlert("错误", "蓝牙打印机连接失败", "OK");
            }
            else if (status == 1)
            {
                _ = Application.Current.MainPage!.DisplayAlert("错误", "无法打印，打印机缺纸！", "OK");
            }
            else if (status == 2)
            {
                _ = Application.Current.MainPage!.DisplayAlert("错误", "无法打印，打印机开盖！", "OK");
            }
            else
            {
                _ = Application.Current.MainPage!.DisplayAlert("错误", "打印失败", "OK");
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
                _ = Application.Current.MainPage!.DisplayAlert("错误", ex.Message, "OK");
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
                _ = Application.Current.MainPage!.DisplayAlert("错误", ex.Message, "OK");
            });
            return false;
        }
        //if (!zpSDK.Connect(info.Address))
        //{
        //    Toast.MakeText(Platform.AppContext!, "连接蓝牙设备失败！" + info.Address, ToastLength.Long)?.Show();
        //    return false;
        //}
        //zpSDK.PrinterStatus();
        //int status = zpSDK.Status;
        //if (status == 1)
        //{
        //    //"缺纸----------";
        //    Toast.MakeText(Platform.AppContext!, "无法打印，打印机缺纸！", ToastLength.Long)?.Show();
        //    return false;
        //}
        //else if (status == 2)
        //{
        //    //"开盖----------";
        //    Toast.MakeText(Platform.AppContext!, "无法打印，打印机开盖！", ToastLength.Long)?.Show();
        //    return false;
        //}
        //try
        //{
        //    zpSDK.PageSetup(info.PageWidth, info.PageHeight);
        //    List<PrintParametersBase> jsonArray = info.PrintParameters;
        //    for (int j = 0; j < jsonArray.Count; j++)
        //    {
        //        PrintParametersBase jsonObject = jsonArray[j];

        //        string drawType = jsonObject.DrawType.ToString().ToUpper();
        //        switch (d, rawType)
        //        {
        //            case "DRAWBOX":
        //                {
        //                    DrawBoxParameters jsonObject2 = (DrawBoxParameters)jsonObject;
        //                    zpSDK.DrawBox(jsonObject2.LineWidth, jsonObject2.TopLeftX, jsonObject2.TopLeftY, jsonObject2.BottomRightX, jsonObject2.BottomRightY);
        //                    break;
        //                }
        //            case "DRAWLINE":
        //                {
        //                    DrawLineParameters jsonObject2 = (DrawLineParameters)jsonObject;
        //                    zpSDK.DrawLine(jsonObject2.LineWidth, jsonObject2.StartX, jsonObject2.StartY, jsonObject2.EndX, jsonObject2.EndY, jsonObject2.FullLine);
        //                    break;
        //                }
        //            case "DRAWTEXT1":
        //                {
        //                    DrawText1Parameters jsonObject2 = (DrawText1Parameters)jsonObject;
        //                    zpSDK.DrawText(jsonObject2.TextX, jsonObject2.TextY, jsonObject2.Text, jsonObject2.FontSize, jsonObject2.Rotate, jsonObject2.Bold, jsonObject2.Reverse, jsonObject2.Underline);
        //                    break;
        //                }
        //            case "DRAWTEXT2":
        //                {
        //                    DrawText2Parameters jsonObject2 = (DrawText2Parameters)jsonObject;
        //                    zpSDK.DrawText(jsonObject2.TextX, jsonObject2.TextY, jsonObject2.Width, jsonObject2.Height, jsonObject2.Text, jsonObject2.FontSize, jsonObject2.Rotate, jsonObject2.Bold, jsonObject2.Underline, jsonObject2.Reverse);
        //                    break;
        //                }
        //            case "DRAWBARCODE":
        //                {
        //                    DrawBarCodeParameters jsonObject2 = (DrawBarCodeParameters)jsonObject;
        //                    zpSDK.DrawBarCode(jsonObject2.StartX, jsonObject2.StartY, jsonObject2.Text, (int)jsonObject2.Type, jsonObject2.Rotate, jsonObject2.Width, jsonObject2.Height);
        //                    break;
        //                }
        //            case "DRAWQRCODE":
        //                {
        //                    DrawQrCodeParameters jsonObject2 = (DrawQrCodeParameters)jsonObject;
        //                    zpSDK.DrawQrCode(jsonObject2.StartX, jsonObject2.StartY, jsonObject2.Text, jsonObject2.Rotate, jsonObject2.Ver, jsonObject2.Lel);
        //                    break;
        //                }
        //            case "DRAWGRAPHIC":
        //                {
        //                    DrawGraphicParameters jsonObject2 = (DrawGraphicParameters)jsonObject;
        //                    int start_x = jsonObject2.StartX;
        //                    int start_y = jsonObject2.StartY;
        //                    int bmp_size_w_percentage = jsonObject2.BmpSizeWPercentage;
        //                    int bmp_size_h_percentage = jsonObject2.BmpSizeHPercentage;
        //                    int rotate = jsonObject2.Rotate;
        //                    int threshold = jsonObject2.Threshold;
        //                    string dithering_type = jsonObject2.DitheringType.ToString().ToUpper();
        //                    string base64 = jsonObject2.Base64;
        //                    byte[]? decodedString = Base64.Decode(base64, Base64Flags.Default);
        //                    Bitmap? decodedByte = BitmapFactory.DecodeByteArray(decodedString, 0, decodedString?.Length ?? 0);
        //                    zpSDK.DrawGraphic(start_x, start_y, bmp_size_w_percentage, bmp_size_h_percentage, rotate, dithering_type, threshold, decodedByte);
        //                    //zpSDK.Draw_Page_Bitmap_(decodedByte,0);
        //                    break;
        //                }
        //            case "DRAWBIGGRAPHIC":
        //                {
        //                    DrawBigGraphicParameters jsonObject2 = (DrawBigGraphicParameters)jsonObject;
        //                    int start_x = jsonObject2.StartX;
        //                    int start_y = jsonObject2.StartY;
        //                    int bmp_size_w_percentage = jsonObject2.BmpSizeWPercentage;
        //                    int bmp_size_h_percentage = jsonObject2.BmpSizeHPercentage;
        //                    int rotate = jsonObject2.Rotate;
        //                    int threshold = jsonObject2.Threshold;
        //                    string dithering_type = jsonObject2.DitheringType.ToString().ToUpper();
        //                    string base64 = jsonObject2.Base64;
        //                    byte[]? decodedString = Base64.Decode(base64, 0);
        //                    if (decodedString == null) break;
        //                    Bitmap? decodedByte = BitmapFactory.DecodeByteArray(decodedString, 0, decodedString.Length);
        //                    if (decodedByte == null) break;
        //                    Bitmap newBitmap = Bitmap.CreateBitmap(decodedByte.Width + start_x, decodedByte.Height + start_y, decodedByte.GetConfig());
        //                    Canvas canvas = new(newBitmap);
        //                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Q && OperatingSystem.IsAndroidVersionAtLeast(29))
        //                        canvas.DrawColor(0);
        //                    canvas.DrawBitmap(decodedByte, start_x, start_y, null);
        //                    decodedByte = newBitmap;
        //                    int w = decodedByte.Width;
        //                    int h = decodedByte.Height;

        //                    float scale_w = bmp_size_w_percentage / 100.0F;
        //                    float scale_h = bmp_size_h_percentage / 100.0F;
        //                    Matrix matrix = new();
        //                    matrix.PostScale(scale_w, scale_h);
        //                    matrix.PostRotate(rotate);
        //                    decodedByte = Bitmap.CreateBitmap(decodedByte, 0, 0, w, h, matrix, true);

        //                    using Dithering dithering = new(threshold);

        //                    switch (dithering_type)
        //                    {
        //                        case "ORDERED2BY2BAYER":
        //                            decodedByte = dithering.Ordered2By2Bayer(decodedByte);
        //                            break;
        //                        case "ORDERED3BY3BAYER":
        //                            decodedByte = dithering.Ordered3By3Bayer(decodedByte);
        //                            break;
        //                        case "ORDERED4BY4BAYER":
        //                            decodedByte = dithering.Ordered4By4Bayer(decodedByte);
        //                            break;
        //                        case "ORDERED8BY8BAYER":
        //                            decodedByte = dithering.Ordered8By8Bayer(decodedByte);
        //                            break;
        //                        case "FLOYDSTEINBERG":
        //                            decodedByte = dithering.FloydSteinberg(decodedByte);
        //                            break;
        //                        case "JARVISJUDICENINKE":
        //                            decodedByte = dithering.JarvisJudiceNinke(decodedByte);
        //                            break;
        //                        case "SIERRA":
        //                            decodedByte = dithering.Sierra(decodedByte);
        //                            break;
        //                        case "TWOROWSIERRA":
        //                            decodedByte = dithering.TwoRowSierra(decodedByte);
        //                            break;
        //                        case "SIERRALITE":
        //                            decodedByte = dithering.SierraLite(decodedByte);
        //                            break;
        //                        case "ATKINSON":
        //                            decodedByte = dithering.Atkinson(decodedByte);
        //                            break;
        //                        case "STUCKI":
        //                            decodedByte = dithering.Stucki(decodedByte);
        //                            break;
        //                        case "BURKES":
        //                            decodedByte = dithering.Burkes(decodedByte);
        //                            break;
        //                        case "FALSEFLOYDSTEINBERG":
        //                            decodedByte = dithering.FalseFloydSteinberg(decodedByte);
        //                            break;
        //                        case "SIMPLELEFTTORIGHTERRORDIFFUSION":
        //                            decodedByte = dithering.SimpleLeftToRightErrorDiffusion(decodedByte);
        //                            break;
        //                        case "RANDOMDITHERING":
        //                            decodedByte = dithering.RandomDithering(decodedByte);
        //                            break;
        //                        case "SIMPLETHRESHOLD":
        //                            decodedByte = dithering.SimpleThreshold(decodedByte);
        //                            break;
        //                    }

        //                    zpSDK.Draw_Page_Bitmap_(decodedByte, 0);
        //                    break;
        //                }
        //        }
        //    }
        //}
        //catch (Exception e)
        //{
        //    Toast.MakeText(Platform.AppContext!, e.Message, ToastLength.Long)?.Show();
        //    Log.Error("PrintInfo", e.Message);
        //}

        //Toast.MakeText(Platform.AppContext!, "开始打印", ToastLength.Long)?.Show();
        //zpSDK.Print(0, 0);
        //zpSDK.PrinterStatus();
        //zpSDK.Disconnect();

        //if (ZpBluetoothPrinter.ListData?.Count > 0)
        //{
        //    foreach (object? item in ZpBluetoothPrinter.ListData)
        //    {
        //        System.Diagnostics.Debug.WriteLine(item);
        //        string byteString = item?.ToString() ?? "";
        //        byteString = byteString[1..^1]; // Remove brackets
        //        string[] byteValues = byteString.Split(", ");

        //        sbyte[] byteArray = new sbyte[byteValues.Length];

        //        for (int i = 0; i < byteValues.Length; i++)
        //        {
        //            byteArray[i] = Convert.ToSByte(byteValues[i]);
        //        }

        //        byte[] unsignedByteArray = new byte[byteArray.Length];
        //        Buffer.BlockCopy(byteArray, 0, unsignedByteArray, 0, byteArray.Length);

        //        string decodedString = Encoding.UTF8.GetString(unsignedByteArray);
        //        LastPrintData += decodedString + "\r\n";
        //        System.Diagnostics.Debug.WriteLine("decodedString:\t" + decodedString);
        //    }
        //}

        //ZpBluetoothPrinter.ListData?.Clear();

        //Toast.MakeText(Platform.AppContext!, "结束打印", ToastLength.Long)?.Show();
        //return true;
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
                _ = Application.Current.MainPage!.DisplayAlert("错误", ex.Message, "OK");
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
                _ = Application.Current.MainPage!.DisplayAlert("错误", ex.Message, "OK");
            });
            return false;
        }
    }
}
#endif