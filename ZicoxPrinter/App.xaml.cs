namespace ZicoxPrinter;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();


        string imageManagerDiskCache = Path.Combine(FileSystem.CacheDirectory, "image_manager_disk_cache");

        if (Directory.Exists(imageManagerDiskCache))
        {
            foreach (var imageCacheFile in Directory.EnumerateFiles(imageManagerDiskCache))
            {
                File.Delete(imageCacheFile);
            }
        }
        else
        {
            Directory.CreateDirectory(imageManagerDiskCache);
        }
    }
}
