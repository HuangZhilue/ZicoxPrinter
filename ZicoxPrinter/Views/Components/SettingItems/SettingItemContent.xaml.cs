namespace ZicoxPrinter.Views.Components.SettingItems;

public partial class SettingItemContent : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(SettingItemContent), string.Empty);
    public static readonly BindableProperty IconSourceProperty = BindableProperty.Create(nameof(IconSource), typeof(ImageSource), typeof(SettingItemContent), null);
    public static readonly BindableProperty RightContentProperty = BindableProperty.Create(nameof(RightContent), typeof(View), typeof(SettingItemContent), null);
    public static readonly BindableProperty BottomContentProperty = BindableProperty.Create(nameof(BottomContent), typeof(View), typeof(SettingItemContent), null);
    public static readonly BindableProperty ShowBottomContentProperty = BindableProperty.Create(nameof(ShowBottomContent), typeof(bool), typeof(SettingItemContent), true);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ImageSource IconSource
    {
        get => (ImageSource)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public View RightContent
    {
        get { return (View)GetValue(RightContentProperty); }
        set { SetValue(RightContentProperty, value); }
    }

    public View BottomContent
    {
        get { return (View)GetValue(BottomContentProperty); }
        set { SetValue(BottomContentProperty, value); }
    }

    public bool ShowBottomContent
    {
        get { return (bool)GetValue(ShowBottomContentProperty); }
        set { SetValue(ShowBottomContentProperty, value); }
    }

    public SettingItemContent()
    {
        InitializeComponent();
    }
}