using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._l__ExtLevelEditor;

public class GoToEvent : ExternalProtocol
{
    public override string ProtocolName => "le";

    public LevelHandler LevelHandler { get; set; }
    public WorldGraph WorldGraph { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        var destinationIds = message[5].Split('|');

        character.Level = int.Parse(destinationIds[0]);
        character.SpawnPoint = int.Parse(destinationIds[1]);

        player.SendLevelChange(NetState, LevelHandler, WorldGraph);
    }
}
