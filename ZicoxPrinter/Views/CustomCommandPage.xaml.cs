namespace ZicoxPrinter.Views;

public partial class CustomCommandPage : ContentPage
{
    public CustomCommandPage(CustomCommandViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
