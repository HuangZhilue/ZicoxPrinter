namespace ZicoxPrinter;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

        if (Preferences.Default.ContainsKey(nameof(AppTheme)) && Current != null)
        {
            Current.UserAppTheme = Enum.TryParse<AppTheme>(Preferences.Default.Get(nameof(AppTheme), AppTheme.Unspecified.ToString()), out var theme) ? theme : AppTheme.Unspecified;
        }
        else if (Current != null)
        {
            Current.UserAppTheme = AppTheme.Unspecified;
            Preferences.Default.Set(nameof(AppTheme), AppTheme.Unspecified.ToString());
        }
        if (Current != null)
        {
            Current.RequestedThemeChanged += (s, a) =>
            {
                Current.UserAppTheme = a.RequestedTheme;
                Preferences.Default.Set(nameof(AppTheme), a.RequestedTheme.ToString());
                ICollection<ResourceDictionary> mergedDictionaries = Current.Resources.MergedDictionaries;
                if (mergedDictionaries != null)
                {
                    //mergedDictionaries.Clear();
                    //mergedDictionaries.Add();
                }
            };
        }

        InitCacheDirectory(["image_manager_disk_cache"]);//, "update_apk"]);
        InitDataCacheDirectory(["temp"]);
    }

    private static void InitCacheDirectory(string[] dirNames)
    {
        for (int i = 0; i < dirNames.Length; i++)
        {
            string imageManagerDiskCache = Path.Combine(FileSystem.CacheDirectory, dirNames[i]);

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

    private static void InitDataCacheDirectory(string[] dirNames)
    {
        for (int i = 0; i < dirNames.Length; i++)
        {
            string imageManagerDiskCache = Path.Combine(FileSystem.AppDataDirectory, dirNames[i]);

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
}
