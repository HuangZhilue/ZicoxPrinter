package com.api.myZpSDK;

import android.content.Context;
import android.util.Log;

import java.util.ArrayList;
import java.util.Locale;

public class Printer {
    private static Printer instance;
    private static PrinterCore printerCore;

    private Printer(Context context) {
        printerCore = PrinterCore.getInstance();
        if (printerCore == null)
            printerCore = PrinterCore.initInstance(context);
    }

    public static Printer initInstance(Context context) {
        if (instance == null) {
            instance = new Printer(context);
        }
        return instance;
    }

    public static Printer getInstance() {
        return instance;
    }

    public int print(PrintInfo info) throws Exception {
        if (printerCore == null) return -2;
        if (!printerCore.connect(info.Address)) {
            return -1;
        }
        printerCore.printerStatus();
        int status = printerCore.getStatus();
        if (status != 0) return status;

        printerCore.pageSetup(info.PageWidth, info.PageHeight);

        for (int i = 0; i < info.PrintParameters.size(); i++) {
            PrintParametersBase parametersBase = info.PrintParameters.get(i);
            DrawType drawType = parametersBase.DrawType;
            switch (drawType) {
//                case DrawCommand: {
//                    DrawCommandParameters parameters = (DrawCommandParameters) parametersBase;
//                    printerCore.drawCommand(parameters.Command);
//                    break;
//                }
                case DrawBox: {
                    DrawBoxParameters parameters = (DrawBoxParameters) parametersBase;
                    printerCore.drawBox(parameters.LineWidth, parameters.TopLeftX, parameters.TopLeftY, parameters.BottomRightX, parameters.BottomRightY);
                    break;
                }
                case DrawLine: {
                    DrawLineParameters parameters = (DrawLineParameters) parametersBase;
                    printerCore.drawLine(parameters.LineWidth, parameters.StartX, parameters.StartY, parameters.EndX, parameters.EndY);
                    break;
                }
//                case DrawText: {
//                    DrawTextParameters parameters = (DrawTextParameters) parametersBase;
//                    printerCore.drawText(parameters.TextX, parameters.TextY, parameters.Text, parameters.TextFont, parameters.FontSize, parameters.Rotate);
//                    break;
//                }
                case DrawText1: {
                    DrawText1Parameters parameters = (DrawText1Parameters) parametersBase;
                    printerCore.drawText(parameters.TextX, parameters.TextY, parameters.Text, parameters.FontSize, parameters.Rotate, parameters.Bold, parameters.Reverse, parameters.Underline);
                    break;
                }
                case DrawText2: {
                    DrawText2Parameters parameters = (DrawText2Parameters) parametersBase;
                    printerCore.drawText(parameters.TextX, parameters.TextY, parameters.Width, parameters.Height, parameters.Text, parameters.FontSize, parameters.Rotate, parameters.Bold, parameters.Reverse, parameters.Underline);
                    break;
                }
                case DrawBarCode: {
                    DrawBarCodeParameters parameters = (DrawBarCodeParameters) parametersBase;
                    printerCore.drawBarCode(parameters.StartX, parameters.StartY, parameters.Text, parameters.Type, parameters.Rotate, parameters.LineWidth, parameters.Height);
                    break;
                }
                case DrawQrCode: {
                    DrawQrCodeParameters parameters = (DrawQrCodeParameters) parametersBase;
                    printerCore.drawQrCode(parameters.StartX, parameters.StartY, parameters.Text, parameters.Ver);
                    break;
                }
                case DrawGraphic: {
                    DrawGraphicParameters parameters = (DrawGraphicParameters) parametersBase;
                    printerCore.drawGraphic(parameters.WidthLimit, parameters.HeightLimit, parameters.StartX, parameters.StartY, parameters.BmpSizeWPercentage, parameters.BmpSizeHPercentage, parameters.Rotate, parameters.DitheringType, parameters.Threshold, parameters.Base64);
                    break;
                }
                case DrawBigGraphic: {
                    DrawBigGraphicParameters parameters = (DrawBigGraphicParameters) parametersBase;
                    printerCore.drawBigGraphic(parameters.WidthLimit, parameters.HeightLimit, parameters.StartX, parameters.StartY, parameters.BmpSizeWPercentage, parameters.BmpSizeHPercentage, parameters.Rotate, parameters.DitheringType, parameters.Threshold, parameters.Base64);
                    break;
                }
                default:
                    throw new IllegalStateException("Unexpected value: " + drawType);
            }
        }

        printerCore.print(0, 0);
        printerCore.printerStatus();
        printerCore.disconnect();
        return 0;
    }

    public int printCPCLRuler(String address, int rulerWidth, int rulerHeight) {
        if (printerCore == null) return -2;
        if (!printerCore.connect(address)) {
            return -1;
        }
        printerCore.printerStatus();
        int status = printerCore.getStatus();
        if (status != 0) return status;

        StringBuilder sb = new StringBuilder();
        sb.append(String.format(Locale.getDefault(), "! 0 200 200 %d 1", rulerHeight));
        sb.append(System.lineSeparator());
        sb.append(String.format(Locale.getDefault(), "PAGE-WIDTH %d", rulerWidth));
        sb.append(System.lineSeparator());
        sb.append(String.format(Locale.getDefault(), "LINE 0 0 %d 0 1", rulerWidth));
        sb.append(System.lineSeparator());
        int i = 0;
        while (true) {
            sb.append(String.format(Locale.getDefault(), "LINE %d 0 %d %d 1", i * 8, i * 8, (i % 10 == 0 ? 60 : i % 5 == 0 ? 50 : 40)));
            sb.append(System.lineSeparator());
            if (i % 5 == 0) {
                sb.append(String.format(Locale.getDefault(), "TEXT 0 0 %d 45 %d", (i * 8) + 5, i * 8));
                sb.append(System.lineSeparator());
            }
            if (i * 8 > rulerWidth) break;
            i++;
        }
        i = 0;
        sb.append(String.format(Locale.getDefault(), "LINE 0 0 0 %d 1", rulerHeight));
        sb.append(System.lineSeparator());
        while (true) {
            sb.append(String.format(Locale.getDefault(), "LINE 0 %d %d %d 1", i * 8, (i % 10 == 0 ? 60 : i % 5 == 0 ? 50 : 40), i * 8));
            sb.append(System.lineSeparator());
            if (i % 5 == 0) {
                sb.append(String.format(Locale.getDefault(), "TEXT 0 0 45 %d %d", (i * 8) + 5, i * 8));
                sb.append(System.lineSeparator());
            }
            if (i * 8 > rulerHeight) break;
            i++;
        }
        i = 0;

        sb.append("LINE 120 80 120 120 1");
        sb.append(System.lineSeparator());
        sb.append("LINE 80 120 120 120 1");
        sb.append(System.lineSeparator());
        sb.append("TEXT 0 2 130 90 H: 40");
        sb.append(System.lineSeparator());
        sb.append("TEXT 0 2 200 90 CPCL ruler (8 points/mm)");
        sb.append(System.lineSeparator());
        sb.append("TEXT 0 2 90 130 W: 40");
        sb.append(System.lineSeparator());

        sb.append("PRINT");
        sb.append(System.lineSeparator());
        Log.d("printCPCLRuler", sb.toString());
        printerCore.writeCommand(sb.toString());

        printerCore.printerStatus();
        printerCore.disconnect();
        return 0;
    }

    public int SendCommand(String address, String command) {
        if (printerCore == null) return -2;
        if (!printerCore.connect(address)) {
            return -1;
        }
        printerCore.printerStatus();
        int status = printerCore.getStatus();
        if (status != 0) return status;

        printerCore.writeCommand(command);

        printerCore.printerStatus();
        printerCore.disconnect();
        return 0;
    }

    public int SendCommand(String address, byte[] command) {
        if (printerCore == null) return -2;
        if (!printerCore.connect(address)) {
            return -1;
        }
        printerCore.printerStatus();
        int status = printerCore.getStatus();
        if (status != 0) return status;

        printerCore.writeCommand(command);

        printerCore.printerStatus();
        printerCore.disconnect();
        return 0;
    }

    public class PrintInfo {
        public String Address = "";
        public ArrayList<PrintParametersBase> PrintParameters = new ArrayList<>();
        public int PageWidth = 0;
        public int PageHeight = 0;
    }

    public abstract class PrintParametersBase {
        protected DrawType DrawType;
    }

    public enum DrawType {
        //        DrawCommand,
        DrawBox,
        DrawLine,
        //        DrawText,
        DrawText1,
        DrawText2,
        DrawBarCode,
        DrawQrCode,
        DrawGraphic,
        DrawBigGraphic
    }

//    public class DrawCommandParameters extends PrintParametersBase {
//        public String Command = "";
//
//        public DrawCommandParameters() {
//            DrawType = Printer.DrawType.DrawCommand;
//        }
//    }

    public class DrawBoxParameters extends PrintParametersBase {
        public int LineWidth;
        public int TopLeftX;
        public int TopLeftY;
        public int BottomRightX;
        public int BottomRightY;

        public DrawBoxParameters() {
            DrawType = Printer.DrawType.DrawBox;
        }
    }

    public class DrawLineParameters extends PrintParametersBase {
        public int LineWidth;
        public int StartX;
        public int StartY;
        public int EndX;
        public int EndY;

        public DrawLineParameters() {
            DrawType = Printer.DrawType.DrawLine;
        }
    }

//    public class DrawTextParameters extends PrintParametersBase {
//        public int TextX;
//        public int TextY;
//        public String Text = "";
//        public PrinterCore.TextFont TextFont = PrinterCore.TextFont.EN0;
//        public int FontSize;
//        public PrinterCore.TextRotate Rotate;
//
//        public DrawTextParameters() {
//            DrawType = Printer.DrawType.DrawText;
//        }
//    }

    public class DrawText1Parameters extends PrintParametersBase {
        public int TextX;
        public int TextY;
        public String Text = "";
        public PrinterCore.TextFont FontSize;
        public PrinterCore.TextRotate Rotate;
        public boolean Bold;
        public boolean Reverse;
        public boolean Underline;

        public DrawText1Parameters() {
            DrawType = Printer.DrawType.DrawText1;
        }
    }

    public class DrawText2Parameters extends PrintParametersBase {
        public int TextX;
        public int TextY;
        public int Width;
        public int Height;
        public String Text = "";
        public PrinterCore.TextFont FontSize;
        public PrinterCore.TextRotate Rotate;
        public boolean Bold;
        public boolean Reverse;
        public boolean Underline;

        public DrawText2Parameters() {
            DrawType = Printer.DrawType.DrawText2;
        }
    }

    public class DrawBarCodeParameters extends PrintParametersBase {
        public int StartX;
        public int StartY;
        public PrinterCore.BarCodeType Type = PrinterCore.BarCodeType.CODE39;
        public String Text = "";
        public int LineWidth;
        public int Height;
        public boolean Rotate;

        public DrawBarCodeParameters() {
            DrawType = Printer.DrawType.DrawBarCode;
        }
    }

    public class DrawQrCodeParameters extends PrintParametersBase {
        public int StartX;
        public int StartY;
        public int Ver;
        public String Text = "";

        public DrawQrCodeParameters() {
            DrawType = Printer.DrawType.DrawQrCode;
        }
    }

    public class DrawGraphicParameters extends PrintParametersBase {
        public int WidthLimit;
        public int HeightLimit;
        public int StartX;
        public int StartY;
        public int BmpSizeWPercentage;
        public int BmpSizeHPercentage;
        public int Rotate;
        public int Threshold = 128;
        public PrinterCore.DitheringType DitheringType = PrinterCore.DitheringType.None;
        public String Base64 = "";

        public DrawGraphicParameters() {
            DrawType = Printer.DrawType.DrawGraphic;
        }
    }

    public class DrawBigGraphicParameters extends DrawGraphicParameters {
        public DrawBigGraphicParameters() {
            DrawType = Printer.DrawType.DrawBigGraphic;
        }
    }
}
