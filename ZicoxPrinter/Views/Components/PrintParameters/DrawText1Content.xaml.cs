using System.Collections.ObjectModel;
using ZicoxPrinter.Services.PrinterSDK;

namespace ZicoxPrinter.Views.Components.PrintParameters;

public partial class DrawText1Content : ContentView
{
    public ObservableCollection<TextFont> TextFonts { get; set; } = new(Enum.GetValues(typeof(TextFont)).Cast<TextFont>());
    public ObservableCollection<TextRotate> TextRotates { get; set; } = new(Enum.GetValues(typeof(TextRotate)).Cast<TextRotate>());

    public static readonly BindableProperty ParametersProperty = BindableProperty.Create(nameof(Parameters), typeof(DrawText1Parameters), typeof(DrawText1Content), new DrawText1Parameters());

    public DrawText1Parameters Parameters
    {
        get => (DrawText1Parameters)GetValue(ParametersProperty);
        set => SetValue(ParametersProperty, value);
    }

    public event EventHandler<EventArgs> ParametersChanged = null!;

    public DrawText1Content()
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