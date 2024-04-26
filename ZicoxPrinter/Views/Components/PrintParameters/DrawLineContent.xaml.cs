using ZicoxPrinter.Services.PrinterSDK;

namespace ZicoxPrinter.Views.Components.PrintParameters;

public partial class DrawLineContent : ContentView
{
    public static readonly BindableProperty ParametersProperty = BindableProperty.Create(nameof(Parameters), typeof(DrawLineParameters), typeof(DrawLineParameters), new DrawLineParameters());

    public DrawLineParameters Parameters
    {
        get => (DrawLineParameters)GetValue(ParametersProperty);
        set => SetValue(ParametersProperty, value);
    }

    public event EventHandler<EventArgs> ParametersChanged = null!;

    public DrawLineContent()
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