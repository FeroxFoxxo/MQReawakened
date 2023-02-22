using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._l__ExtLevelEditor;

public class GoToEvent : ExternalProtocol
{
    public override string ProtocolName => "le";

    public ILogger<GoToEvent> Logger { get; set; }
    public WorldHandler WorldHandler { get; set; }
    public WorldGraph WorldGraph { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        var destinationIds = message[5].Split('|');

        var levelId = int.Parse(destinationIds[0]);
        var spawnId = int.Parse(destinationIds[1]);

        character.SetLevel(levelId, 0, spawnId, Logger);

        player.SendLevelChange(NetState, WorldHandler, WorldGraph);
    }
}
