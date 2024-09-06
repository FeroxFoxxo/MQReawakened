using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Web.Abstractions;
using Web.Razor.Services;

namespace Web.Razor;

public class Razor(ILogger<Razor> logger) : WebModule(logger)
{
    public override void AddServices(IServiceCollection services, Module[] modules) => services.AddSingleton<PagesService>();
}
