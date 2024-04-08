namespace ZicoxPrinter.Views;

public partial class ImagePrinterPage : ContentPage
{
    public ImagePrinterPage(ImagePrinterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
