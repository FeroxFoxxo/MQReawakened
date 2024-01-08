using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._d__DescriptionHandler;

public class RequestGoText : ExternalProtocol
{
    public override string ProtocolName => "dg";

    public ILogger<RequestGoText> Logger { get; set; }

    public override void Run(string[] message)
    {
        var gameObjectId = message[5];

        // %dg is not used anywhere else but portals, so the all id fields are unnecessary.
        SendXt("dg", gameObjectId, 1, 1, 1);
    }
}
