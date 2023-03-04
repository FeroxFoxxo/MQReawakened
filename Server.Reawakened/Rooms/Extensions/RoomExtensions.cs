using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Rooms.Extensions;

public static class RoomExtensions
{
    public static void SendSyncEvent(this Room room, SyncEvent syncEvent, Player sentPlayer = null)
    {
        foreach (
            var player in
            from player in room.Players.Values
            where sentPlayer == null || player.UserId != sentPlayer.UserId
            select player
        )
            player.SendSyncEventToPlayer(syncEvent);
    }
}
