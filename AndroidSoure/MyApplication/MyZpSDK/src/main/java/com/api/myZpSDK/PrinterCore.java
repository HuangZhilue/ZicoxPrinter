package com.api.myZpSDK;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Matrix;
import android.util.Base64;
import android.util.Log;

import java.io.ByteArrayOutputStream;
import java.nio.charset.StandardCharsets;

import zpSDK.zpSDK.zpBluetoothPrinter;

public class PrinterCore {
    private static PrinterCore instance;
    public final zpBluetoothPrinter zpSDK;

    private PrinterCore(Context context) {
        zpSDK = new zpBluetoothPrinter(context);
    }

    public static PrinterCore initInstance(Context context) {
        if (instance == null) {
            instance = new PrinterCore(context);
        }
        return instance;
    }

    public static PrinterCore getInstance() {
        return instance;
    }

    public boolean connect(String address) {
        return zpSDK.connect(address);
    }

    public void disconnect() {
        zpSDK.disconnect();
    }

    public void printerStatus() {
        zpSDK.printerStatus();
    }

    public Integer getStatus() {
        return zpSDK.GetStatus();
    }

    public void pageSetup(int pageWight, int pageHeight) {
        zpSDK.pageSetup(pageWight, pageHeight);
    }

    public void writeCommand(String command) {
        zpSDK.Write(command.getBytes(StandardCharsets.UTF_8));
    }

    public void writeCommand(byte[] command) {
        zpSDK.Write(command);
    }

    public void drawBox(int lineWidth, int top_left_x, int top_left_y, int bottom_right_x, int bottom_right_y) {
        zpSDK.drawBox(lineWidth, top_left_x, top_left_y, bottom_right_x, bottom_right_y);
    }

    public void drawLine(int lineWidth, int start_x, int start_y, int end_x, int end_y) {
        zpSDK.drawLine(lineWidth, start_x, start_y, end_x, end_y, true);
    }

    public void drawText(int text_x, int text_y, String text, TextFont fontSize, TextRotate rotate, boolean bold, boolean reverse, boolean underline) {
        zpSDK.drawText(text_x, text_y, text, fontSize.getValue(), rotate.getValue(), bold ? 1 : 0, reverse, underline);
    }

    public void drawText(int text_x, int text_y, int width, int height, String text, TextFont fontSize, TextRotate rotate, boolean bold, boolean reverse, boolean underline) {
        zpSDK.drawText(text_x, text_y, width, height, text, fontSize.getValue(), rotate.getValue(), bold ? 1 : 0, underline, reverse);
    }

//    public void drawText(int text_x, int text_y, String text, TextFont font, int text_size, TextRotate rotate) {
//        String cmd = "T";
//        if (rotate == TextRotate.VT || rotate == TextRotate.T90) {
//            cmd = "VT";
//        }
//
//        if (rotate == TextRotate.T180) {
//            cmd = "T180";
//        }
//
//        if (rotate == TextRotate.T270) {
//            cmd = "T270";
//        }
//
//        String str = String.format(Locale.getDefault(), "%s %s %s %d %d %s\r\n", cmd, font.getValue(), text_size, text_x, text_y, text);
//        byte[] byteStr = (byte[]) null;
//
//        try {
//            byteStr = str.getBytes("gbk");
//        } catch (UnsupportedEncodingException var23) {
//        }
//
//        if (byteStr != null) {
//            zpSDK.Write(byteStr);
//        }
//    }

    public void drawBarCode(int start_x, int start_y, String text, BarCodeType type, boolean rotate, int lineWidth, int height) {
        zpSDK.drawBarCode(start_x, start_y, text, type.getValue(), rotate, lineWidth, height);
    }

    public void drawQrCode(int start_x, int start_y, String text, int ver) {
        zpSDK.drawQrCode(start_x, start_y, text, 0, ver < 0 ? 0 : Math.min(ver, 32), 0);
    }

    public void drawGraphic(int widthLimit, int heightLimit, int start_x, int start_y, int bmp_size_w_percentage, int bmp_size_h_percentage, int rotate, DitheringType dithering_type, int threshold, String base64Image) {
        Bitmap decodedByte = processImage(widthLimit, heightLimit, 0, 0, bmp_size_w_percentage, bmp_size_h_percentage, rotate, threshold, dithering_type, base64Image);
        zpSDK.drawGraphic(start_x, start_y, 0, 0, decodedByte);
    }

    public void drawBigGraphic(int widthLimit, int heightLimit, int start_x, int start_y, int bmp_size_w_percentage, int bmp_size_h_percentage, int rotate, DitheringType dithering_type, int threshold, String base64Image) {
        Log.w("PrinterCore", "drawBigGraphic: widthLimit = " + widthLimit + ", heightLimit = " + heightLimit + ", start_x = " + start_x + ", start_y = " + start_y + ", bmp_size_w_percentage = " + bmp_size_w_percentage + ", bmp_size_h_percentage = " + bmp_size_h_percentage + ", rotate = " + rotate + ", threshold = " + threshold + ", dithering_type = " + dithering_type.toString());
        Bitmap decodedByte = processImage(widthLimit, heightLimit, start_x, start_y, bmp_size_w_percentage, bmp_size_h_percentage, rotate, threshold, dithering_type, base64Image);
        if (decodedByte != null)
            Log.w("PrinterCore", "drawBigGraphic: " + decodedByte.getWidth() + " " + decodedByte.getHeight());
        zpSDK.Draw_Page_Bitmap_(decodedByte, 0);
    }

    public boolean print(int horizontal, int skip) {
        return zpSDK.print(horizontal, skip);
    }

    public static Bitmap processImage(int widthLimit, int heightLimit, int start_x, int start_y, int bmp_size_w_percentage, int bmp_size_h_percentage, int rotate, int threshold, DitheringType dithering_type, String base64Image) {
        try {
            byte[] decodedString = Base64.decode(base64Image, Base64.DEFAULT);
            Bitmap decodedByte = BitmapFactory.decodeByteArray(decodedString, 0, decodedString.length);

            Bitmap newBitmap = Bitmap.createBitmap(decodedByte.getWidth() + start_x, decodedByte.getHeight() + start_y, decodedByte.getConfig());
            Canvas canvas = new Canvas(newBitmap);
            canvas.drawColor(-1);
            canvas.drawBitmap(decodedByte, start_x, start_y, null);
            decodedByte = newBitmap;
            int w = decodedByte.getWidth();
            int h = decodedByte.getHeight();

            float scale_w = bmp_size_w_percentage / 100.0F;
            float scale_h = bmp_size_h_percentage / 100.0F;
            Matrix matrix = new Matrix();
            matrix.postScale(scale_w, scale_h);
            matrix.postRotate(rotate);
            decodedByte = Bitmap.createBitmap(decodedByte, 0, 0, w, h, matrix, true);

            Dithering dithering = new Dithering(threshold < 0 ? 0 : Math.min(threshold, 255));

            switch (dithering_type) {
                case Ordered2By2Bayer:
                    decodedByte = dithering.ordered2By2Bayer(decodedByte);
                    break;
                case Ordered3By3Bayer:
                    decodedByte = dithering.ordered3By3Bayer(decodedByte);
                    break;
                case Ordered4By4Bayer:
                    decodedByte = dithering.ordered4By4Bayer(decodedByte);
                    break;
                case Ordered8By8Bayer:
                    decodedByte = dithering.ordered8By8Bayer(decodedByte);
                    break;
                case FloydSteinberg:
                    decodedByte = dithering.floydSteinberg(decodedByte);
                    break;
                case JarvisJudiceNinke:
                    decodedByte = dithering.jarvisJudiceNinke(decodedByte);
                    break;
                case Sierra:
                    decodedByte = dithering.sierra(decodedByte);
                    break;
                case TwoRowSierra:
                    decodedByte = dithering.twoRowSierra(decodedByte);
                    break;
                case SierraLite:
                    decodedByte = dithering.sierraLite(decodedByte);
                    break;
                case Atkinson:
                    decodedByte = dithering.atkinson(decodedByte);
                    break;
                case Stucki:
                    decodedByte = dithering.stucki(decodedByte);
                    break;
                case Burkes:
                    decodedByte = dithering.burkes(decodedByte);
                    break;
                case FalseFloydSteinberg:
                    decodedByte = dithering.falseFloydSteinberg(decodedByte);
                    break;
                case SimpleLeftToRightErrorDiffusion:
                    decodedByte = dithering.simpleLeftToRightErrorDiffusion(decodedByte);
                    break;
                case RandomDithering:
                    decodedByte = dithering.randomDithering(decodedByte);
                    break;
                case SimpleThreshold:
                    decodedByte = dithering.simpleThreshold(decodedByte);
                    break;
                case None:
                default:
                    // 处理默认情况
                    break;
            }
            widthLimit = widthLimit <= 0 ? decodedByte.getWidth() : Math.min(widthLimit, decodedByte.getWidth());
            heightLimit = heightLimit <= 0 ? decodedByte.getHeight() : Math.min(heightLimit, decodedByte.getHeight());

            return Bitmap.createBitmap(decodedByte, 0, 0, widthLimit, heightLimit);

        } catch (Exception e) {
            e.printStackTrace();
            Log.e("processImage", "processImage: " + e.getMessage());
        }
        return null;
    }

    public static String processImageToBase64(int widthLimit, int heightLimit, int start_x, int start_y, int bmp_size_w_percentage, int bmp_size_h_percentage, int rotate, int threshold, DitheringType dithering_type, String base64Image) {
        Bitmap bitmap = processImage(widthLimit, heightLimit, start_x, start_y, bmp_size_w_percentage, bmp_size_h_percentage, rotate, threshold, dithering_type, base64Image);
        if (bitmap == null) return "";
        ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
        bitmap.compress(Bitmap.CompressFormat.PNG, 100, outputStream);
        byte[] byteArray = outputStream.toByteArray();
        return Base64.encodeToString(byteArray, Base64.DEFAULT);
    }

    public enum TextFont {
        EN0(0),
        EN1(1),
        EN2(2),
        EN3(3),
        EN4(4),
        EN5(5),
        EN6(6),
        EN7(7),
        CN16(55),
        CN24(56);

        private final int value;

        TextFont(int value) {
            this.value = value;
        }

        public int getValue() {
            return value;
        }
    }

    public enum TextRotate {
        T(0),
        VT(1),
        T90(1),
        T180(2),
        T270(3);
        private final int value;

        TextRotate(int value) {
            this.value = value;
        }

        public int getValue() {
            return value;
        }
    }

    public enum BarCodeType {
        CODE39(0),
        CODE128(1),
        CODE93(2),
        CODABAR(3),
        EAN8(4),
        EAN13(5),
        UPC_A(6),
        UPC_E(7),
        I2OF5(8);
        private final int value;

        BarCodeType(int value) {
            this.value = value;
        }

        public int getValue() {
            return value;
        }
    }

    public enum DitheringType {
        None,
        Ordered2By2Bayer,
        Ordered3By3Bayer,
        Ordered4By4Bayer,
        Ordered8By8Bayer,
        FloydSteinberg,
        JarvisJudiceNinke,
        Sierra,
        TwoRowSierra,
        SierraLite,
        Atkinson,
        Stucki,
        Burkes,
        FalseFloydSteinberg,
        SimpleLeftToRightErrorDiffusion,
        RandomDithering,
        SimpleThreshold
    }
}
