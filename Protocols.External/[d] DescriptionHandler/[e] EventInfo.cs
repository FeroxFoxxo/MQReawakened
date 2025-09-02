using Server.Reawakened.Network.Protocols;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._d__DescriptionHandler;
public class EventInfo : ExternalProtocol
{
    public override string ProtocolName => "de";

    public EventPrefabs EventPrefabs { get; set; }

    public override void Run(string[] message)
    {
        if (EventPrefabs.EventInfo != null && Player.TempData.FirstLogin)
            SendXt("de", EventPrefabs.EventInfo.ToString());
    }
}
