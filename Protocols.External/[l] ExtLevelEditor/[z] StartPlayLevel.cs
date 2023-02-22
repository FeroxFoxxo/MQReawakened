using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlayRoom : ExternalProtocol
{
    public override string ProtocolName => "lz";

    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var room = WorldHandler.GetRoomFromLevelId(player.GetLevelId());

        player.JoinRoom(NetState, room, out var reason);

        SendXt("lz", reason.GetJoinReasonError(), room.LevelInfo.LevelId, room.LevelInfo.Name);

        var tribe = room.LevelInfo.Tribe;
        NetState.DiscoverTribe(tribe);
    }
}
