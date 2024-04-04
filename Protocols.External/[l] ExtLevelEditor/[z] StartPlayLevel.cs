using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.Colliders;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlayRoom : ExternalProtocol
{
    public override string ProtocolName => "lz";

    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message)
    {
        Player.QuickJoinRoom(Player.GetLevelId(), WorldHandler, out var reason);

        SendXt("lz", reason.GetJoinReasonError(), Player.Room.LevelInfo.LevelId, Player.Room.LevelInfo.Name);

        var tribe = Player.Room.LevelInfo.Tribe;
        Player.DiscoverTribe(tribe);

        Player.Room.AddCollider(new PlayerCollider(Player));
    }
}
