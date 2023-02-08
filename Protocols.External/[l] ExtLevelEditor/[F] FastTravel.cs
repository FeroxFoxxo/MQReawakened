using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._l__ExtLevelEditor;

public class FastTravel : ExternalProtocol
{
    public override string ProtocolName => "lF";

    public LevelHandler LevelHandler { get; set; }
    public WorldGraph WorldGraph { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();
        
        var newLevelId = WorldGraph.GetDestinationFromPortal(int.Parse(message[5]), int.Parse(message[6]));

        character.Level = newLevelId;

        player.SendLevelChange(NetState, LevelHandler, WorldGraph);
    }
}
