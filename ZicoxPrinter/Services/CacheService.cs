using System.Reflection;

namespace ZicoxPrinter.Services;

public static class CacheService
{
    public static string ImageManagerDiskCacheDirectory { get; } = Path.Combine(FileSystem.CacheDirectory, "image_manager_disk_cache");
    public static string TempDataDirectory { get; } = Path.Combine(FileSystem.AppDataDirectory, "temp");

    public static void InitAllCacheDirectories()
    {
        foreach (string directory in GetAllCacheDirectories())
        {
            InitCacheDirectories(directory);
        }
    }

    public static void InitCacheDirectories(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory)) return;
        string cacheDir = Path.Combine(directory);

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

    public static long GetAllCacheSize()
    {
        long totalSize = 0;
        foreach (string directory in GetAllCacheDirectories())
        {
            totalSize += GetCacheSize(directory);
        }
        return totalSize;
    }

    public static long GetCacheSize(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory)) return 0;
        string cacheDir = Path.Combine(directory);

        if (!Directory.Exists(cacheDir)) return 0;

        return CalculateFolderSize(cacheDir);
    }

    private static long CalculateFolderSize(string folderPath)
    {
        long totalSize = 0;

        foreach (string filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
        {
            FileInfo fileInfo = new(filePath);
            totalSize += fileInfo.Length;
        }

        foreach (string subfolderPath in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories))
        {
            totalSize += CalculateFolderSize(subfolderPath);
        }

        return totalSize;
    }

    private static List<string> GetAllCacheDirectories()
    {
        Type staticType = typeof(CacheService);
        PropertyInfo[] properties = staticType.GetProperties(BindingFlags.Static | BindingFlags.Public);
        List<string> directories = [];
        foreach (PropertyInfo property in properties)
        {
            object? value = property.GetValue(null); // Pass null for static properties
            Console.WriteLine($"{property.Name}: {value}");
            if (value is string directory)
                directories.Add(directory);
        }

        return directories;
    }
}
