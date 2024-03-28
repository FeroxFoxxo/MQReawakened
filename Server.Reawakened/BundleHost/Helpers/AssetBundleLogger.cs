using AssetStudio;
using Microsoft.Extensions.Logging;

namespace Server.Reawakened.BundleHost.Helpers;

public class AssetBundleLogger(Microsoft.Extensions.Logging.ILogger logger) : AssetStudio.ILogger
{
    public void Log(LoggerEvent loggerEvent, string message)
    {
        switch (loggerEvent)
        {
            case LoggerEvent.Verbose:
                logger.LogTrace("{Message}", message);
                break;
            case LoggerEvent.Debug:
                logger.LogDebug("{Message}", message);
                break;
            case LoggerEvent.Info:
                logger.LogInformation("{Message}", message);
                break;
            case LoggerEvent.Warning:
                logger.LogWarning("{Message}", message);
                break;
            case LoggerEvent.Error:
                logger.LogError("{Message}", message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(loggerEvent), loggerEvent, null);
        }
    }
}
