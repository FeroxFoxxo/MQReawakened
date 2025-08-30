namespace Server.Base.Core.Extensions;

public static class EnvironmentExt
{
    public static bool IsContainerOrNonInteractive()
    {
        var inContainer = string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        var nonInteractive = Console.IsInputRedirected;

        return inContainer || nonInteractive;
    }
}


