using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using System.Text.Json;

namespace Server.Base.Core.Extensions;

public static class GetConfigs
{
    private const string ConfigDir = "Configs";

    public static void LoadConfigs(this IServiceCollection services, Type config, ILogger logger)
    {
        try
        {
            using var stream = GetFile.GetFileStream(GetFileName(config), ConfigDir, FileMode.Open);
            services.AddSingleton(config,
                JsonSerializer.Deserialize(stream, config) ?? throw new InvalidCastException());
            logger.LogTrace("Config: Found {Name} in '{Directory}'", config.Name, Path.GetDirectoryName(stream.Name));
        }
        catch (FileNotFoundException)
        {
            services.AddSingleton(config);
            logger.LogTrace("Config: {Name} was not found, creating!", config.Name);
        }
        catch (UnauthorizedAccessException e)
        {
            services.AddSingleton(config);
            logger.LogDebug("Config: {Name} could not be created! {Error}", config.Name, e.Message);
        }
    }

    public static void SaveConfigs(this IServiceProvider services, IEnumerable<Module> modules, ILogger logger)
    {
        var serialisationOptions = new JsonSerializerOptions { WriteIndented = true };

        foreach (var config in services.GetRequiredServices<IRwConfig>(modules))
        {
            var configName = config.GetType().Name;
            using var stream = GetFile.GetFileStream(GetFileName(config.GetType()), ConfigDir, FileMode.Create);
            JsonSerializer.Serialize(stream, config, config.GetType(), serialisationOptions);
            logger.LogTrace("Config: {Name} has been saved", configName);
        }
    }

    public static string GetFileName(Type config) =>
        $"{config.Name.Replace("RwConfig", string.Empty)}.json";
}
