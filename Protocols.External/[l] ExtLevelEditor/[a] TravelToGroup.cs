using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._l__ExtLevelEditor;
public class TravelToGroup : ExternalProtocol
{
    public override string ProtocolName => "la";

    public WorldGraph WorldGraph { get; set; }

    public WorldHandler WorldHandler { get; set; }

    public DatabaseContainer DatabaseContainer { get; set; }

    public ILogger<TravelToGroup> Logger { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var leaderName = Player.TempData.Group.GetLeaderName();
        var leader = DatabaseContainer.GetPlayerByName(leaderName);

        var levelId = leader.Character.LevelData.LevelId;

        character.SetLevel(levelId, Logger);

        Player.SendLevelChange(WorldHandler, WorldGraph);

        Logger.LogError("Travelling to group leaders are not implemented yet!");
    }
}
