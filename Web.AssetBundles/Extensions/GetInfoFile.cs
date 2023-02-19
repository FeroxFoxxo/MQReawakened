using Microsoft.Extensions.Logging;
using Server.Base.Core.Helpers;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Extensions;

public static class GetInfoFile
{
    public static string GetWebPlayerInfoFile(this AssetBundleConfig config, AssetBundleStaticConfig sConfig,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        config.WebPlayerInfoFile = TryGetInfoFile($"Web Player '{sConfig.DefaultWebPlayerCacheLocation}'",
            config.WebPlayerInfoFile, logger);

        if (config.WebPlayerInfoFile == config.CacheInfoFile)
        {
            logger.LogError("Web player cache and saved directory should not be the same! Skipping...");
            config.WebPlayerInfoFile = string.Empty;
        }

        if (!config.WebPlayerInfoFile.ToLower().Contains("appdata"))
        {
            logger.LogError("Web player cache has to be in the AppData/LocalLow folder! Skipping...");
            config.WebPlayerInfoFile = string.Empty;
        }

        return config.WebPlayerInfoFile;
    }

    public static string TryGetInfoFile(string cacheName, string defaultFile,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        logger.LogInformation("Getting The {Type} Cache Directory", cacheName);

        try
        {
            defaultFile = SetFileValue.SetIfNotNull(defaultFile, $"Get the {cacheName} '__info' Cache File",
                $"{cacheName} Info File (__info)\0__info\0");
        }
        catch
        {
            // ignored
        }

        while (true)
        {
            cacheName = cacheName.ToLower();

            if (string.IsNullOrEmpty(defaultFile) || !defaultFile.EndsWith("__info"))
            {
                logger.LogError("Please enter the absolute file path for the {Type} '__info' cache file.", cacheName);
                defaultFile = Console.ReadLine() ?? string.Empty;
                continue;
            }

            break;
        }

        logger.LogDebug("Got the {Type} cache directory: {Directory}", cacheName, Path.GetDirectoryName(defaultFile));

        return defaultFile;
    }
}
