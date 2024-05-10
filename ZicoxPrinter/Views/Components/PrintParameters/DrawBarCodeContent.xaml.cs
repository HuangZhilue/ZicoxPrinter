using System.Collections.ObjectModel;
using ZicoxPrinter.Services.PrinterSDK;

namespace ZicoxPrinter.Views.Components.PrintParameters;

public partial class DrawBarCodeContent : ContentView
{
    public ObservableCollection<BarcodeType> BarcodeTypes { get; set; } = new(Enum.GetValues(typeof(BarcodeType)).Cast<BarcodeType>());

    public static readonly BindableProperty ParametersProperty = BindableProperty.Create(nameof(Parameters), typeof(DrawBarCodeParameters), typeof(DrawBarCodeContent), new DrawBarCodeParameters());

    public event EventHandler<EventArgs> ParametersChanged = null!;

    public DrawBarCodeParameters Parameters
    {
        get => (DrawBarCodeParameters)GetValue(ParametersProperty);
        set => SetValue(ParametersProperty, value);
    }

    public DrawBarCodeContent()
    {
        InitializeComponent();
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        ParametersChanged?.Invoke(Parameters, EventArgs.Empty);
    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        ParametersChanged?.Invoke(Parameters, EventArgs.Empty);
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
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