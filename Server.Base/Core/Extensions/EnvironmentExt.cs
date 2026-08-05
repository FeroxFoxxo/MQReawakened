namespace Server.Base.Core.Extensions;

public static class EnvironmentExt
{
    public static bool IsContainer() => string.Equals(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            "true",
            StringComparison.OrdinalIgnoreCase) && !IsConsoleInteractive();

    public static bool IsContainerOrNonInteractive()
    {
        var inContainer = IsContainer();

        var nonInteractive = Console.IsInputRedirected;

        return (inContainer || nonInteractive) && !IsConsoleInteractive();
    }

    public static bool IsConsoleInteractive()
    {
        var envVar = string.Equals(
            Environment.GetEnvironmentVariable("INTERACTIVE_CONSOLE"),
            "true",
            StringComparison.OrdinalIgnoreCase);
        
        return envVar;
    }
}


