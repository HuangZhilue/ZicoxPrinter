#if ANDROID

using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Widget;
using Com.Api.ZpSDK;
using Base64 = Android.Util.Base64;

namespace ZicoxPrinter.Services.PrinterSDK;

public class Printer
{
    /**
     * 获取所有蓝牙设备
     */
    public static Dictionary<string, string> GetDevices(string Device, string DeviceRegexStr)
    {
        Toast.MakeText(Platform.AppContext!, "获取打印机信息", ToastLength.Long)?.Show();
        if (Android.App.Application.Context.GetSystemService(Context.BluetoothService) is not BluetoothManager bluetoothManager)
        {
            Toast.MakeText(Platform.AppContext!, "BluetoothManager Is Null", ToastLength.Long)?.Show();
            return [];
        }
        BluetoothAdapter myBluetoothAdapter = bluetoothManager.Adapter!;
        if (myBluetoothAdapter is null)
        {
            Toast.MakeText(Platform.AppContext!, "当前设备没有找到蓝牙适配器", ToastLength.Long)?.Show();
            return [];
        }

        if (Build.VERSION.SdkInt > BuildVersionCodes.R && AndroidX.Core.Content.ContextCompat.CheckSelfPermission(Platform.AppContext, Manifest.Permission.BluetoothConnect) != Permission.Granted)
        {
            Toast.MakeText(Platform.AppContext!, "没有蓝牙权限", ToastLength.Long)?.Show();

            return [];
        }
        if (!myBluetoothAdapter.IsEnabled)
        {
            Intent enableBtIntent = new(BluetoothAdapter.ActionRequestEnable);
            enableBtIntent.AddFlags(ActivityFlags.NewTask);
            Platform.AppContext.StartActivity(enableBtIntent, new Bundle(2));
        }

        List<BluetoothDevice> pairedDevices = [.. myBluetoothAdapter.BondedDevices];
        if (pairedDevices.Count == 0)
        {
            Toast.MakeText(Platform.AppContext!, "当前设备没有匹配到蓝牙设备", ToastLength.Long)?.Show();

            return [];
        }
        Dictionary<string, string> map = [];
        Toast.MakeText(Platform.AppContext!, "正在搜索可用的打印机...", ToastLength.Long)?.Show();
        string devStr = Device;
        string devStrRegex = DeviceRegexStr;

        foreach (BluetoothDevice device in pairedDevices)
        {
            if (string.IsNullOrWhiteSpace(device.Name) || string.IsNullOrWhiteSpace(device.Address)) continue;
            if ((!string.IsNullOrWhiteSpace(devStr) && device.Name.Contains(devStr))
            || (!string.IsNullOrWhiteSpace(devStrRegex) && System.Text.RegularExpressions.Regex.IsMatch(device.Name, devStrRegex)))
            {
                map.Add(device.Name, device.Address);
            }
        }

        if (map.Count == 0)
        {
            Toast.MakeText(Platform.AppContext!, "获取蓝牙设备出现异常", ToastLength.Long)?.Show();
            Log.Error(nameof(GetDevices), "获取蓝牙设备出现异常");
        }
        return map;
    }

    public static bool Print(PrintInfo info)
    {
        //实例化
        using ZpBluetoothPrinter zpSDK = new(Platform.AppContext);

        if (!zpSDK.Connect(info.Address))
        {
            Toast.MakeText(Platform.AppContext!, "连接蓝牙设备失败！" + info.Address, ToastLength.Long)?.Show();
            return false;
        }
        zpSDK.PrinterStatus();
        int status = zpSDK.Status;
        if (status == 1)
        {
            //"缺纸----------";
            Toast.MakeText(Platform.AppContext!, "无法打印，打印机缺纸！", ToastLength.Long)?.Show();
            return false;
        }
        else if (status == 2)
        {
            //"开盖----------";
            Toast.MakeText(Platform.AppContext!, "无法打印，打印机开盖！", ToastLength.Long)?.Show();
            return false;
        }
        try
        {
            zpSDK.PageSetup(info.PageWidth, info.PageHeight);
            List<PrintParametersBase> jsonArray = info.PrintParameters;
            for (int j = 0; j < jsonArray.Count; j++)
            {
                PrintParametersBase jsonObject = jsonArray[j];

                string drawType = jsonObject.DrawType.ToString().ToUpper();
                switch (drawType)
                {
                    case "DRAWBOX":
                        {
                            DrawBoxParameters jsonObject2 = (DrawBoxParameters)jsonObject;
                            zpSDK.DrawBox(jsonObject2.LineWidth, jsonObject2.TopLeftX, jsonObject2.TopLeftY, jsonObject2.BottomRightX, jsonObject2.BottomRightY);
                            break;
                        }
                    case "DRAWLINE":
                        {
                            DrawLineParameters jsonObject2 = (DrawLineParameters)jsonObject;
                            zpSDK.DrawLine(jsonObject2.LineWidth, jsonObject2.StartX, jsonObject2.StartY, jsonObject2.EndX, jsonObject2.EndY, jsonObject2.FullLine);
                            break;
                        }
                    case "DRAWTEXT1":
                        {
                            DrawText1Parameters jsonObject2 = (DrawText1Parameters)jsonObject;
                            zpSDK.DrawText(jsonObject2.TextX, jsonObject2.TextY, jsonObject2.Text, jsonObject2.FontSize, jsonObject2.Rotate, jsonObject2.Bold, jsonObject2.Reverse, jsonObject2.Underline);
                            break;
                        }
                    case "DRAWTEXT2":
                        {
                            DrawText2Parameters jsonObject2 = (DrawText2Parameters)jsonObject;
                            zpSDK.DrawText(jsonObject2.TextX, jsonObject2.TextY, jsonObject2.Width, jsonObject2.Height, jsonObject2.Text, jsonObject2.FontSize, jsonObject2.Rotate, jsonObject2.Bold, jsonObject2.Underline, jsonObject2.Reverse);
                            break;
                        }
                    case "DRAWBARCODE":
                        {
                            DrawBarCodeParameters jsonObject2 = (DrawBarCodeParameters)jsonObject;
                            zpSDK.DrawBarCode(jsonObject2.StartX, jsonObject2.StartY, jsonObject2.Text, jsonObject2.Type, jsonObject2.Rotate, jsonObject2.LineWidth, jsonObject2.Height);
                            break;
                        }
                    case "DRAWQRCODE":
                        {
                            DrawQrCodeParameters jsonObject2 = (DrawQrCodeParameters)jsonObject;
                            zpSDK.DrawQrCode(jsonObject2.StartX, jsonObject2.StartY, jsonObject2.Text, jsonObject2.Rotate, jsonObject2.Ver, jsonObject2.Lel);
                            break;
                        }
                    case "DRAWGRAPHIC":
                        {
                            DrawGraphicParameters jsonObject2 = (DrawGraphicParameters)jsonObject;
                            int start_x = jsonObject2.StartX;
                            int start_y = jsonObject2.StartY;
                            int bmp_size_w_percentage = jsonObject2.BmpSizeWPercentage;
                            int bmp_size_h_percentage = jsonObject2.BmpSizeHPercentage;
                            int rotate = jsonObject2.Rotate;
                            int threshold = jsonObject2.Threshold;
                            string dithering_type = jsonObject2.DitheringType.ToString().ToUpper();
                            string base64 = jsonObject2.Base64;
                            byte[]? decodedString = Base64.Decode(base64, Base64Flags.Default);
                            Bitmap? decodedByte = BitmapFactory.DecodeByteArray(decodedString, 0, decodedString?.Length ?? 0);
                            zpSDK.DrawGraphic(start_x, start_y, bmp_size_w_percentage, bmp_size_h_percentage, rotate, dithering_type, threshold, decodedByte);
                            //zpSDK.Draw_Page_Bitmap_(decodedByte,0);
                            break;
                        }
                    case "DRAWBIGGRAPHIC":
                        {
                            DrawBigGraphicParameters jsonObject2 = (DrawBigGraphicParameters)jsonObject;
                            int start_x = jsonObject2.StartX;
                            int start_y = jsonObject2.StartY;
                            int bmp_size_w_percentage = jsonObject2.BmpSizeWPercentage;
                            int bmp_size_h_percentage = jsonObject2.BmpSizeHPercentage;
                            int rotate = jsonObject2.Rotate;
                            int threshold = jsonObject2.Threshold;
                            string dithering_type = jsonObject2.DitheringType.ToString().ToUpper();
                            string base64 = jsonObject2.Base64;
                            byte[] decodedString = Base64.Decode(base64, 0);
                            Bitmap? decodedByte = BitmapFactory.DecodeByteArray(decodedString, 0, decodedString.Length);

                            Bitmap newBitmap = Bitmap.CreateBitmap(decodedByte.Width + start_x, decodedByte
                                    .Height + start_y, decodedByte.GetConfig());
                            Canvas canvas = new(newBitmap);
                            canvas.DrawColor(0);
                            canvas.DrawBitmap(decodedByte, start_x, start_y, null);
                            decodedByte = newBitmap;
                            int w = decodedByte.Width;
                            int h = decodedByte.Height;

                            float scale_w = bmp_size_w_percentage / 100.0F;
                            float scale_h = bmp_size_h_percentage / 100.0F;
                            Matrix matrix = new();
                            matrix.PostScale(scale_w, scale_h);
                            matrix.PostRotate(rotate);
                            decodedByte = Bitmap.CreateBitmap(decodedByte, 0, 0, w, h, matrix, true);

                            using Dithering dithering = new(threshold);

                            switch (dithering_type)
                            {
                                case "ORDERED2BY2BAYER":
                                    decodedByte = dithering.Ordered2By2Bayer(decodedByte);
                                    break;
                                case "ORDERED3BY3BAYER":
                                    decodedByte = dithering.Ordered3By3Bayer(decodedByte);
                                    break;
                                case "ORDERED4BY4BAYER":
                                    decodedByte = dithering.Ordered4By4Bayer(decodedByte);
                                    break;
                                case "ORDERED8BY8BAYER":
                                    decodedByte = dithering.Ordered8By8Bayer(decodedByte);
                                    break;
                                case "FLOYDSTEINBERG":
                                    decodedByte = dithering.FloydSteinberg(decodedByte);
                                    break;
                                case "JARVISJUDICENINKE":
                                    decodedByte = dithering.JarvisJudiceNinke(decodedByte);
                                    break;
                                case "SIERRA":
                                    decodedByte = dithering.Sierra(decodedByte);
                                    break;
                                case "TWOROWSIERRA":
                                    decodedByte = dithering.TwoRowSierra(decodedByte);
                                    break;
                                case "SIERRALITE":
                                    decodedByte = dithering.SierraLite(decodedByte);
                                    break;
                                case "ATKINSON":
                                    decodedByte = dithering.Atkinson(decodedByte);
                                    break;
                                case "STUCKI":
                                    decodedByte = dithering.Stucki(decodedByte);
                                    break;
                                case "BURKES":
                                    decodedByte = dithering.Burkes(decodedByte);
                                    break;
                                case "FALSEFLOYDSTEINBERG":
                                    decodedByte = dithering.FalseFloydSteinberg(decodedByte);
                                    break;
                                case "SIMPLELEFTTORIGHTERRORDIFFUSION":
                                    decodedByte = dithering.SimpleLeftToRightErrorDiffusion(decodedByte);
                                    break;
                                case "RANDOMDITHERING":
                                    decodedByte = dithering.RandomDithering(decodedByte);
                                    break;
                                case "SIMPLETHRESHOLD":
                                    decodedByte = dithering.SimpleThreshold(decodedByte);
                                    break;
                            }


                            zpSDK.Draw_Page_Bitmap_(decodedByte, 0);
                            break;
                        }
                }
            }
        }
        catch (System.Exception e)
        {
            Toast.MakeText(Platform.AppContext!, e.Message, ToastLength.Long)?.Show();
            Log.Error("PrintInfo", e.Message);
        }

        Toast.MakeText(Platform.AppContext!, "开始打印", ToastLength.Long)?.Show();
        zpSDK.Print(0, 0);
        zpSDK.PrinterStatus();
        zpSDK.Disconnect();
        Toast.MakeText(Platform.AppContext!, "结束打印", ToastLength.Long)?.Show();
        return true;
    }
}
#endif