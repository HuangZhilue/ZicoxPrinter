namespace ZicoxPrinter.Services;

public static class MauiAssetService
{
    public static async Task<string> LoadStringAsset(string assetName)
    {
        using Stream stream = await FileSystem.OpenAppPackageFileAsync(assetName);
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
