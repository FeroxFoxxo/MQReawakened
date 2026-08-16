using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._d__DescriptionHandler;

public class RequestUIText : ExternalProtocol
{
    public override string ProtocolName => "du";

    public ILogger<RequestUIText> Logger { get; set; }

    public override void Run(string[] message)
    {
        var uiName = message[5];

        Logger.LogDebug("Description {UIName} requested", uiName);
        SendXt("du", uiName, 1, 1, 1);
    }
}
