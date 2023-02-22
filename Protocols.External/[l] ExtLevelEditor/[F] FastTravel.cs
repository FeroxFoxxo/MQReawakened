using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._l__ExtLevelEditor;

public class FastTravel : ExternalProtocol
{
    public override string ProtocolName => "lF";

    public ILogger<FastTravel> Logger { get; set; }
    public WorldHandler WorldHandler { get; set; }
    public WorldGraph WorldGraph { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        var regionId = int.Parse(message[5]);
        var levelId = int.Parse(message[6]);
        var newLevelId = WorldGraph.GetDestinationFromPortal(regionId, levelId);

        character.SetLevel(newLevelId, Logger);

        player.SendLevelChange(NetState, WorldHandler, WorldGraph);
    }
}
