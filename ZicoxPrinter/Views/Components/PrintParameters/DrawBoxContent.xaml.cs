using ZicoxPrinter.Services.PrinterSDK;

namespace ZicoxPrinter.Views.Components.PrintParameters;

public partial class DrawBoxContent : ContentView
{
    public static readonly BindableProperty ParametersProperty = BindableProperty.Create(
        nameof(Parameters),
        typeof(DrawBoxParameters),
        typeof(DrawBoxContent),
        new DrawBoxParameters());

    public event EventHandler<EventArgs> ParametersChanged = null!;

    public DrawBoxParameters Parameters
    {
        get => (DrawBoxParameters)GetValue(ParametersProperty);
        set => SetValue(ParametersProperty, value);
    }

    public DrawBoxContent()
    {
        InitializeComponent();
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        ParametersChanged?.Invoke(Parameters, EventArgs.Empty);
    }

    public event EventHandler<EventArgs> ParametersRemoving = null!;

    [RelayCommand]
    public void RemoveThis()
    {
        ParametersRemoving?.Invoke(Parameters, EventArgs.Empty);
    }
}