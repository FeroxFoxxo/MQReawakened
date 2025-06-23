using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Protocols.External;
using Protocols.System;
using Server.Base.Core.Abstractions;
using Server.Base.Logging;
using Server.Reawakened;
using System.Linq;
using Web.Apps;
using Web.Launcher;
using Web.Razor;
using Web.WebPlayer;

namespace Init;

public static class ImportModules
{
    public static Module[] GetModules()
    {
        var modules = new[]
        {
            typeof(Server.Web.Web),
            typeof(Server.Base.ServerModule),
            typeof(Reawakened),
            typeof(SysProtocol),
            typeof(XtProtocol),
            typeof(Launcher),
            typeof(Apps),
            typeof(Razor),
            typeof(WebPlayer)
        };

        var services = new ServiceCollection();

        services.AddLogging(l =>
        {
            l.AddProvider(new LoggerProvider());
            l.SetMinimumLevel(LogLevel.Trace);
        });

        foreach (var type in modules)
            services.AddSingleton(type);

        var provider = services.BuildServiceProvider();

        return [.. modules.Select(module => provider.GetRequiredService(module) as Module)];
    }
}
