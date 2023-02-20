using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Logging;
using Server.Web.Abstractions;
using System;
using System.Threading.Tasks;

namespace Init;

public class Initialize
{
    public static async Task Main()
    {
        var logger = new Logger("Initialization");

        try
        {
            logger.LogInformation("============ Launching =============");

            var builder = WebApplication.CreateBuilder();

            logger.LogDebug("Getting modules");
            var modules = GetModules(logger);

            logger.LogDebug("Importing modules");
            InitializeModules(modules, builder, logger);

            logger.LogDebug("Building application");
            var app = builder.Build();
            logger.LogInformation("Application built");

            logger.LogDebug("Configuring application");
            ConfigureApp(modules, app, logger);

            logger.LogInformation("======== Running Application =======");

            await app.RunAsync();
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "Could not start application!");
        }
    }

    private static Module[] GetModules(ILogger logger)
    {
        var modules = ImportModules.GetModules();

        foreach (var module in modules)
        {
            logger.LogInformation("Imported {ModuleInfo}", module.GetModuleInformation());

            if (module.Contributors.Length <= 0)
                continue;

            logger.LogTrace("    Contributed By:      ");
            logger.LogTrace("        {Contributors}", string.Join(", ", module.Contributors));
        }

        logger.LogInformation("Fetched {ModuleCount} modules", modules.Length);

        return modules;
    }

    private static void InitializeModules(Module[] modules, WebApplicationBuilder builder, ILogger logger)
    {
        logger.LogDebug("Initializing logging");
        foreach (var startup in modules)
            startup.AddLogging(builder.Logging);
        logger.LogInformation("Successfully initialized logging");

        logger.LogDebug("Initializing services");
        foreach (var startup in modules)
            startup.AddServices(builder.Services, modules);
        logger.LogInformation("Successfully initialized services");

        logger.LogDebug("Configuring services");
        foreach (var startup in modules)
            startup.ConfigureServices(builder.Configuration, builder.Services);
        logger.LogInformation("Successfully configured services");

        logger.LogDebug("Initializing web services");

        var controller = builder.Services.AddControllers();

        foreach (var module in modules)
        {
            if (module is WebModule m)
                m.InitializeWeb(builder);

            controller.AddApplicationPart(module.GetType().Assembly);
        }

        logger.LogInformation("Successfully initialized web services");
    }

    private static void ConfigureApp(Module[] modules, WebApplication app, ILogger logger)
    {
        foreach (var startup in modules)
        {
            startup.PostBuild(app.Services, modules);

            if (startup is WebModule module)
                module.PostWebBuild(app);
        }

        logger.LogInformation("Successfully post built application");
    }
}
