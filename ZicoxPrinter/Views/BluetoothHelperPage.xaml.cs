namespace ZicoxPrinter.Views;

public partial class BluetoothHelperPage : ContentPage
{
    public BluetoothHelperPage(BluetoothHelperViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}