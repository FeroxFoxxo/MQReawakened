namespace Server.Base.Core.Extensions;

public static class EnvironmentExt
{
    public static bool IsContainer() => string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    public static bool IsContainerOrNonInteractive()
    {
        var inContainer = IsContainer();

        var nonInteractive = Console.IsInputRedirected;

        return inContainer || nonInteractive;
    }
    
    public static bool IsConsoleInteractive() => string.Equals(
            Environment.GetEnvironmentVariable("INTERACTIVE_CONSOLE"),
            "true",
            StringComparison.OrdinalIgnoreCase);
}


