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
        Task.Run(() =>
        {
            InitCacheDirectories(["image_manager_disk_cache"], FileSystem.CacheDirectory);
            InitCacheDirectories(["temp"], FileSystem.AppDataDirectory);
        });
    }

    private static void InitCacheDirectories(string[] dirNames, string baseDirectory)
    {
        foreach (var dirName in dirNames)
        {
            string cacheDir = Path.Combine(baseDirectory, dirName);

            if (Directory.Exists(cacheDir))
            {
                foreach (var cacheFile in Directory.EnumerateFiles(cacheDir))
                {
                    try
                    {
                        File.Delete(cacheFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(cacheDir);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
