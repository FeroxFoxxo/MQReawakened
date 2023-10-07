using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.Apps;

public class Apps : WebModule
{
    public override string[] Contributors { get; } = ["Ferox"];

    public Apps(ILogger<Apps> logger) : base(logger)
    {
    }
}
