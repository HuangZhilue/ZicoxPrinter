namespace ZicoxPrinter.Views;

public partial class CustomJsonPrinterPage : ContentPage
{
    public CustomJsonPrinterPage(CustomJsonPrinterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void DrawParametersContent_CustomPrintParametersChanged(object sender, EventArgs e)
    {
        //if (sender == null) return;
        //if (sender is not PrintParametersBase parameter) return;
        //Debug.WriteLine(nameof(DrawParametersContent_CustomPrintParametersChanged) + "\t:\t" + parameter.DrawType.ToString());
        //if (BindingContext is CustomJsonPrinterViewModel viewModel)
        //{
        //    await viewModel.CustomPrintParametersChanged(sender).ConfigureAwait(false);
        //}
    }
}
