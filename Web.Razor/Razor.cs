using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.Razor;

public class Razor : WebModule
{
    public override string[] Contributors { get; } = { "Ferox" };

    public Razor(ILogger<Razor> logger) : base(logger)
    {
    }
}
