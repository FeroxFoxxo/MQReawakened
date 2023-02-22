using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlay : ExternalProtocol
{
    public override string ProtocolName => "ly";

    public WorldHandler WorldHandler { get; set; }
    public WorldGraph WorldGraph { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        player.SendLevelChange(NetState, WorldHandler, WorldGraph);
    }
}
