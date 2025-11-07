using System.Collections.ObjectModel;
using ZicoxPrinter.Services;
using ZicoxPrinter.Services.PrinterSDK;

namespace ZicoxPrinter.Views.Components;

public partial class DrawParametersContent : ContentView
{
    private List<DrawType> DrawTypes { get; set; } =
        [
            .. Enum.GetValues<DrawType>()
                .Cast<DrawType>()
                .Where(t => t is not DrawType.DrawCommand && t is not DrawType.DrawBigGraphic)
        ];

    public static readonly BindableProperty CustomPrintParametersProperty = BindableProperty.Create(
        nameof(CustomPrintParameters),
        typeof(ObservableCollection<PrintParametersBase>),
        typeof(ObservableCollection<PrintParametersBase>),
        null
    );

    public ObservableCollection<PrintParametersBase> CustomPrintParameters
    {
        get => (ObservableCollection<PrintParametersBase>)GetValue(CustomPrintParametersProperty);
        set => SetValue(CustomPrintParametersProperty, value);
    }

    public event EventHandler<EventArgs> CustomPrintParametersChanged = null!;

    public DrawParametersContent()
    {
        InitializeComponent();
    }

    [RelayCommand]
    public async Task AddNewParameterAsync()
    {
        string action = await ApplicationEx.DisplayActionSheetOnUIThreadAsync(
            AppResources.新增参数,
            AppResources.取消,
            null,
            [.. DrawTypes.Select(t => t.ToString())]
        );
        Debug.WriteLine("Action: " + action);
        if (string.IsNullOrWhiteSpace(action) || action == AppResources.取消)
            return;
        if (!Enum.TryParse(typeof(DrawType), action, true, out object? obj))
            return;
        if (obj == null || obj is not DrawType drawType)
            return;

        CustomPrintParameters ??= [];

        switch (drawType)
        {
            case DrawType.DrawBox:
                CustomPrintParameters.Add(new DrawBoxParameters());
                break;
            case DrawType.DrawLine:
                CustomPrintParameters.Add(new DrawLineParameters());
                break;
            case DrawType.DrawText1:
                CustomPrintParameters.Add(new DrawText1Parameters());
                break;
            case DrawType.DrawText2:
                CustomPrintParameters.Add(new DrawText2Parameters());
                break;
            case DrawType.DrawBarCode:
                CustomPrintParameters.Add(new DrawBarCodeParameters());
                break;
            case DrawType.DrawQrCode:
                CustomPrintParameters.Add(new DrawQrCodeParameters());
                break;
            case DrawType.DrawGraphic:
                CustomPrintParameters.Add(new DrawGraphicParameters());
                break;
            case DrawType.DrawBigGraphic:
            case DrawType.DrawCommand:
            default:
                break;
        }
    }

    [RelayCommand]
    public void DeleteParameter(PrintParametersBase parameter)
    {
        if (parameter == null)
            return;
        DrawContent_ParametersRemoving(parameter, EventArgs.Empty);
    }

    private void DrawContent_ParametersChanged(object sender, EventArgs e)
    {
        CustomPrintParametersChanged?.Invoke(sender, e);
    }

    private async void DrawContent_ParametersRemoving(object sender, EventArgs e)
    {
        if (sender == null)
            return;
        if (sender is not PrintParametersBase parameter)
            return;
        bool answer = await ApplicationEx
            .DisplayAlertOnUIThreadAsync(
                AppResources.提示,
                $"{AppResources.确定移除该参数}: {parameter.DrawType}",
                AppResources.是,
                AppResources.否
            )
            .ConfigureAwait(false);
        Debug.WriteLine("Answer: " + answer);
        if (answer)
        {
            CustomPrintParameters.Remove(parameter);
            DrawContent_ParametersChanged(parameter, e);
        }
    }
}

public class DrawTypeTemplateSelector : DataTemplateSelector
{
    public DataTemplate DrawBoxTemplate { get; set; } = null!;
    public DataTemplate DrawLineTemplate { get; set; } = null!;
    public DataTemplate DrawText1Template { get; set; } = null!;
    public DataTemplate DrawText2Template { get; set; } = null!;
    public DataTemplate DrawBarCodeTemplate { get; set; } = null!;
    public DataTemplate DrawQrCodeTemplate { get; set; } = null!;
    public DataTemplate DrawGraphicTemplate { get; set; } = null!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is DrawBoxParameters)
        {
            return DrawBoxTemplate;
        }
        if (item is DrawLineParameters)
        {
            return DrawLineTemplate;
        }
        if (item is DrawText1Parameters)
        {
            return DrawText1Template;
        }
        if (item is DrawText2Parameters)
        {
            return DrawText2Template;
        }
        if (item is DrawBarCodeParameters)
        {
            return DrawBarCodeTemplate;
        }
        if (item is DrawQrCodeParameters)
        {
            return DrawQrCodeTemplate;
        }
        if (item is DrawGraphicParameters)
        {
            return DrawGraphicTemplate;
        }
        // Add more conditions for other DrawTypes if needed

        return null!;
    }
}
