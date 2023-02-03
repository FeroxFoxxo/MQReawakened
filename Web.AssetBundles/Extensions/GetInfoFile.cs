using Microsoft.Extensions.Logging;
using Server.Base.Core.Helpers.Internal;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Extensions;

public static class GetInfoFile
{
    public static string GetWebPlayerInfoFile(this AssetBundleConfig config,
        Microsoft.Extensions.Logging.ILogger logger) =>
        config.WebPlayerInfoFile = TryGetInfoFile($"Web Player '{config.DefaultWebPlayerCacheLocation}'",
            config.WebPlayerInfoFile, logger);

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
