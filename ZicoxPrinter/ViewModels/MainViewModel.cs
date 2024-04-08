#if ANDROID
using ZicoxPrinter.Services.PrinterSDK;
#endif
namespace ZicoxPrinter.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty]
    private ImageSource image = null!;

    private string ImageBase64 { get; set; } = string.Empty;

    [RelayCommand]
    public async Task PickAndShow()
    {
        try
        {
#if ANDROID
            await Application.Current.MainPage.DisplayAlert("Alert", "You have been alerted", "OK");
#endif
            FileResult? photo = await MediaPicker.PickPhotoAsync();
            if (photo == null) return;
            string newFile = Path.Combine(FileSystem.CacheDirectory, "image_manager_disk_cache", photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
            {
                await stream.CopyToAsync(newStream);
            }

            Image = ImageSource.FromFile(newFile);

            using MemoryStream memoryStream = new();
            FileStream fileStream = File.OpenRead(newFile);
            fileStream.CopyTo(memoryStream);
            byte[] bytes = memoryStream.ToArray();
            ImageBase64 = Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FilePicker Error: {ex.Message}");
            // The user canceled or something went wrong
            if (Application.Current is null || Application.Current.MainPage is null) return;
            await Application.Current.MainPage.DisplayAlert("错误", ex.Message, "OK");
        }
    }

    [RelayCommand]
    public void Print()
    {
#if ANDROID
        Dictionary<string, string> devices = Printer.GetDevices("CC3", @"^(((C[CS])|(XT))\d+)$");

        if (devices.Count > 0)
        {
            PrintInfo printInfo = new()
            {
                Address = devices.First().Value,
                PageHeight = 200,
                PageWidth = 200,
                PrintParameters = [
                    new DrawBigGraphicParameters()
                    {
                        Base64 = ImageBase64,
                        StartX = 0,
                        StartY = 0,
                        BmpSizeHPercentage = 100,
                        BmpSizeWPercentage = 100,
                        Rotate = 0,
                        Threshold = 128,
                        DitheringType = DitheringType.Burkes
                    }
                ]
            };

            Printer.Print(printInfo);
        }
#endif
    }
}
