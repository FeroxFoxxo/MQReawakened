using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlayRoom : ExternalProtocol
{
    public override string ProtocolName => "lz";

    public WorldHandler WorldHandler { get; set; }
    public DatabaseContainer DatabaseContainer { get; set; }
    public ILogger<StartPlayRoom> Logger { get; set; }

    public override void Run(string[] message)
    {
        var room = WorldHandler.GetRoomFromLevelId(Player.GetLevelId(), Player);

        Player.JoinRoom(room, out var reason);

        SendXt("lz", reason.GetJoinReasonError(), room.LevelInfo.LevelId, room.LevelInfo.Name);

        var tribe = room.LevelInfo.Tribe;
        Player.DiscoverTribe(tribe);

        RemoveLastCheckpoint();

        Logger.LogInformation("Loading into [{levelName}]! PlayerCount: {playerCount}", room.LevelInfo.InGameName, DatabaseContainer.GetAllPlayers().Count);
    }

    public void RemoveLastCheckpoint()
    {
        if (Player.TempData.LastCheckpoint != null)
            Player.TempData.LastCheckpoint = null;       
    }
}
