namespace ZicoxPrinter.Services.PrinterSDK;

/// <summary>
/// 打印信息
/// </summary>
public class PrintInfo
{
    public string Address { get; set; } = string.Empty;
    public List<PrintParametersBase> PrintParameters { get; set; } = [];
    public int PageWidth { get; set; }
    public int PageHeight { get; set; }
}

public abstract class PrintParametersBase
{
    public DrawType DrawType { get; protected set; }
}

public enum DrawType
{
    DrawCommand,
    DrawBox,
    DrawLine,
    DrawText1,
    DrawText2,
    DrawBarCode,
    DrawQrCode,
    DrawGraphic,
    DrawBigGraphic
}

public enum TextFont
{
    EN0 = 0,
    EN1 = 1,
    EN2 = 2,
    EN3 = 3,
    EN4 = 4,
    EN5 = 5,
    EN6 = 6,
    EN7 = 7,
    CN16 = 55,
    CN24 = 56
}

public enum TextRotate
{
    T = 0,
    VT = 1,
    //T90 = 1,
    T180 = 2,
    T270 = 3
}

public enum BarcodeType
{
    Code39 = 0,
    Code128 = 1,
    Code93 = 2,
    Codabar = 3,
    EAN8 = 4,
    EAN13 = 5,
    UPCA = 6,
    UPCE = 7,
    I2OF5 = 8
}

public enum DitheringType
{
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

public class DrawBoxParameters : PrintParametersBase
{
    public int LineWidth { get; set; } = 1;
    public int TopLeftX { get; set; }
    public int TopLeftY { get; set; }
    public int BottomRightX { get; set; }
    public int BottomRightY { get; set; }

    public DrawBoxParameters()
    {
        DrawType = DrawType.DrawBox;
    }
}

public class DrawLineParameters : PrintParametersBase
{
    public int LineWidth { get; set; } = 1;
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int EndX { get; set; }
    public int EndY { get; set; }

    public DrawLineParameters()
    {
        DrawType = DrawType.DrawLine;
    }
}

public class DrawText1Parameters : PrintParametersBase
{
    public int TextX { get; set; }
    public int TextY { get; set; }
    public string Text { get; set; } = string.Empty;
    public TextFont FontSize { get; set; } = TextFont.EN0;
    public TextRotate Rotate { get; set; } = TextRotate.T;
    public bool Bold { get; set; }
    public bool Reverse { get; set; }
    public bool Underline { get; set; }

    public DrawText1Parameters()
    {
        DrawType = DrawType.DrawText1;
    }
}

public class DrawText2Parameters : PrintParametersBase
{
    public int TextX { get; set; }
    public int TextY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Text { get; set; } = string.Empty;
    public TextFont FontSize { get; set; } = TextFont.EN1;
    public TextRotate Rotate { get; set; } = TextRotate.T;
    public bool Bold { get; set; }
    public bool Reverse { get; set; }
    public bool Underline { get; set; }

    public DrawText2Parameters()
    {
        DrawType = DrawType.DrawText2;
    }
}

public class DrawBarCodeParameters : PrintParametersBase
{
    public int StartX { get; set; }
    public int StartY { get; set; }
    public BarcodeType Type { get; set; } = BarcodeType.Code39;
    public string Text { get; set; } = string.Empty;
    public int LineWidth { get; set; } = 2;
    public int Height { get; set; } = 80;
    public bool Rotate { get; set; }

    public DrawBarCodeParameters()
    {
        DrawType = DrawType.DrawBarCode;
    }
}

public class DrawQrCodeParameters : PrintParametersBase
{
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int Ver { get; set; } = 3;
    public string Text { get; set; } = string.Empty;

    public DrawQrCodeParameters()
    {
        DrawType = DrawType.DrawQrCode;
    }
}

public class DrawGraphicParameters : PrintParametersBase
{
    public int WidthLimit { get; set; } = -1;
    public int HeightLimit { get; set; } = -1;
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int BmpSizeWPercentage { get; set; } = 100;
    public int BmpSizeHPercentage { get; set; } = 100;
    public int Rotate { get; set; }
    public int Threshold { get; set; } = 128;
    public DitheringType DitheringType { get; set; } = DitheringType.SimpleThreshold;
    public string Base64 { get; set; } = string.Empty;

    public DrawGraphicParameters()
    {
        DrawType = DrawType.DrawGraphic;
    }
}

public class DrawBigGraphicParameters : DrawGraphicParameters
{
    public DrawBigGraphicParameters()
    {
        DrawType = DrawType.DrawBigGraphic;
    }
}