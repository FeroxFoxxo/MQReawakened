using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;

namespace Protocols.External;

public class XtProtocol(ILogger<XtProtocol> logger) : Module(logger)
{
    public override string[] Contributors { get; } = ["Ferox"];
}
