using Microsoft.Extensions.Logging;

namespace Server.Base.Core.Extensions;
public static class ConsoleExt
{
    public static string ReadOrEnv(string envVarName, ILogger logger, string defaultVal = "")
    {
        var envVar = Environment.GetEnvironmentVariable(envVarName);

        var value = !string.IsNullOrEmpty(envVar) ? envVar :
            !string.IsNullOrEmpty(defaultVal) ? defaultVal :
            !GetOsType.IsUnix() ? Console.ReadLine() :
            throw new ArgumentException($"You need to specify '{envVarName}' in your environmental variables.");

        logger?.LogInformation("Setting {EnvVarName} as {Value}", envVarName, value);

        return value;
    }
}
