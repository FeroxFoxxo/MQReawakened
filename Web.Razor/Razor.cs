using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.Razor;

public class Razor(ILogger<Razor> logger) : WebModule(logger)
{
    public override string[] Contributors { get; } = ["Ferox"];
}
