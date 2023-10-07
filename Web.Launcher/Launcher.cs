using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.Launcher;

public class Launcher : WebModule
{
    public override string[] Contributors { get; } = ["Ferox"];

    public Launcher(ILogger<Launcher> logger) : base(logger)
    {
    }
}
