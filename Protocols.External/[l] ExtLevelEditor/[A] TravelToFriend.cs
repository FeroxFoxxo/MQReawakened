using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._l__ExtLevelEditor;
public class TravelToFriend : ExternalProtocol
{
    public override string ProtocolName => "lA";

    public WorldHandler WorldHandler { get; set; }
    public PlayerContainer PlayerContainer { get; set; }

    public override void Run(string[] message)
    {
        var playerName = message[5];
        var otherPlayer = PlayerContainer.GetPlayerByName(playerName);

        var levelId = otherPlayer.Character.LevelId;
        var spawnId = otherPlayer.Character.SpawnPointId;

        WorldHandler.ChangePlayerRoom(Player, levelId, spawnId);
    }
}
