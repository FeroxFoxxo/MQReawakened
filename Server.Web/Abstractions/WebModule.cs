using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;

namespace Server.Web.Abstractions;

public abstract class WebModule(ILogger logger) : Module(logger)
{
    public virtual void InitializeWeb(WebApplicationBuilder builder)
    {
    }

    public virtual void PostWebBuild(WebApplication app)
    {
    }

    public override string GetModuleInformation() => $"{base.GetModuleInformation()} (WEB)";
}
