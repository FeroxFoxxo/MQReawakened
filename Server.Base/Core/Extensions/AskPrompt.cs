using Microsoft.Extensions.Logging;

namespace Server.Base.Core.Extensions;

public static class AskPrompt
{
    public static bool Ask(this ILogger logger, string message)
    {
        logger.LogWarning(message);
        logger.LogError("Are you sure you want to continue? (y/n)");

        var choice = Console.ReadKey().KeyChar;
        Console.WriteLine();

        if (choice == 'y')
            return true;

        if (choice != 'n')
            logger.LogWarning("Invalid output detected, found: '{char}'. Skipping...", choice);

        return false;
    }
}
