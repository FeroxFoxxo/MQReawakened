using Microsoft.Extensions.Logging;

namespace Server.Base.Core.Extensions;
public static class ConsoleExt
{
    public static string ReadOrEnv(string envVarName, ILogger logger, string defaultVal = "")
    {
        var envVar = Environment.GetEnvironmentVariable(envVarName);

        var value = !string.IsNullOrEmpty(envVar) ? envVar :
            !string.IsNullOrEmpty(defaultVal) ? defaultVal :
            EnvironmentExt.IsContainerOrNonInteractive() ?
                throw new ArgumentException($"You need to specify '{envVarName}' in your environmental variables.") :
                Console.ReadLine();

        logger?.LogInformation("Setting {EnvVarName} as {Value}", envVarName, value);

        return value;
    }

    public static string ReadLineOrDefault(ILogger logger = null, string defaultVal = null)
    {
        if (EnvironmentExt.IsContainerOrNonInteractive())
        {
            logger?.LogDebug("Non-interactive; returning default for ReadLine: {DefaultValue}", defaultVal);
            return defaultVal;
        }

        return Console.ReadLine();
    }

    public static bool TryReadLine(out string value, ILogger logger = null)
    {
        value = ReadLineOrDefault(logger, null);
        return !string.IsNullOrEmpty(value);
    }
}
