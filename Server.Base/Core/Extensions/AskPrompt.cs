using Microsoft.Extensions.Logging;

namespace Server.Base.Core.Extensions;

public static class AskPrompt
{
    public static bool Ask(this ILogger logger, string message, bool defaultValue)
    {
        logger.LogWarning("{Message}", message);
        logger.LogError("Are you sure you want to continue? (y/n) (Default: {DefaultValue})", defaultValue ? 'Y' : 'N');

        var reqLine = ConsoleExt.ReadLineOrDefault(logger, null);

        if (string.IsNullOrEmpty(reqLine))
        {
            logger.LogInformation("Setting as default value: {DefaultValue}", defaultValue);
            return defaultValue;
        }

        var line = reqLine.ToLower();

        if (line.StartsWith('y'))
            return true;

        if (!line.StartsWith('n'))
        {
            logger.LogWarning("Invalid output detected, found: '{Character}'. Defaulting to {DefaultValue}...",
                line, defaultValue);
            return defaultValue;
        }

        return false;
    }
}
