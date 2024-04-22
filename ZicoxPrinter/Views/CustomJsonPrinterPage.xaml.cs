namespace ZicoxPrinter.Views;

public partial class CustomJsonPrinterPage : ContentPage
{
    public CustomJsonPrinterPage(CustomJsonPrinterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
