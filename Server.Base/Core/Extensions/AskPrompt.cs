using Microsoft.Extensions.Logging;

namespace Server.Base.Core.Extensions;

public static class AskPrompt
{
    public static bool Ask(this ILogger logger, string message, bool defaultValue)
    {
        logger.LogWarning("{Message}", message);
        logger.LogError("Are you sure you want to continue? (y/n) (Default: {DefaultValue})", defaultValue ? 'Y' : 'N');

        var choice = char.ToLower(Console.ReadKey().KeyChar);

        if (choice is '\n' or '\r')
            choice = ' ';

        Console.WriteLine();

        if (choice == 'y')
            return true;

        if (choice != 'n')
        {
            logger.LogWarning("Invalid output detected, found: '{Character}'. Defaulting to {DefaultValue}...",
                choice, defaultValue);
            return defaultValue;
        }

        return false;
    }
}
