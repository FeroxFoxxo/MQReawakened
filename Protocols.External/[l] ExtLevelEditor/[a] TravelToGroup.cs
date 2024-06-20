using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._l__ExtLevelEditor;
public class TravelToGroup : ExternalProtocol
{
    public override string ProtocolName => "la";

    public WorldHandler WorldHandler { get; set; }
    public PlayerContainer PlayerContainer { get; set; }

    public override void Run(string[] message)
    {
        var leaderName = Player.TempData.Group.GetLeaderName();
        var leader = PlayerContainer.GetPlayerByName(leaderName);

        var levelId = leader.Character.LevelId;

        WorldHandler.ChangePlayerRoom(Player, levelId);
    }
}
