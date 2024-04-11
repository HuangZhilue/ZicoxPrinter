namespace ZicoxPrinter.Services.PrinterSDK_New;

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
    public DrawType DrawType { get; set; }
}

public enum DrawType
{
    DrawBox,
    DrawLine,
    DrawText1,
    DrawText2,
    DrawBarCode,
    DrawQrCode,
    DrawGraphic,
    DrawBigGraphic
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
    public int LineWidth { get; set; }
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
    public int LineWidth { get; set; }
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int EndX { get; set; }
    public int EndY { get; set; }
    public bool FullLine { get; set; }

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
    public int FontSize { get; set; }
    public int Rotate { get; set; }
    public int Bold { get; set; }
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
    public int FontSize { get; set; }
    public int Rotate { get; set; }
    public int Bold { get; set; }
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
    public int Type { get; set; }
    public int Height { get; set; }
    public string Text { get; set; } = string.Empty;
    public int LineWidth { get; set; }
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
    public int Ver { get; set; }
    public int Lel { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Rotate { get; set; }

    public DrawQrCodeParameters()
    {
        DrawType = DrawType.DrawQrCode;
    }
}

public class DrawGraphicParameters : PrintParametersBase
{
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int BmpSizeWPercentage { get; set; }
    public int BmpSizeHPercentage { get; set; }
    public int Rotate { get; set; }
    public int Threshold { get; set; } = 128;
    public DitheringType DitheringType { get; set; } = DitheringType.None;
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