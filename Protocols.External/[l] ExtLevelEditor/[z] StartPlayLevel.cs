using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Enums;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlayRoom : ExternalProtocol
{
    public override string ProtocolName => "lz";

    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message)
    {
        Player.Character.StatusEffects.UpdateStatus();

        Player.TemporaryInvincibility(2);
		
        Player.QuickJoinRoom(Player.GetLevelId(), WorldHandler, out var reason);

        var room = Player.Room;

        if (reason != JoinReason.Accepted || room == null)
        {
            SendXt("lz", reason.GetJoinReasonError(), -1, string.Empty);
            return;
        }

        var levelInfo = room.LevelInfo;

        SendXt("lz", reason.GetJoinReasonError(), levelInfo.LevelId, levelInfo.Name);

        Player.DiscoverTribe(levelInfo.Tribe);
    }
}
