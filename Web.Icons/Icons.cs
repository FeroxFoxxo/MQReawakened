using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.Icons;

public class Icons(ILogger<Icons> logger) : WebModule(logger)
{
    public override string[] Contributors { get; } = ["Ferox"];
}
