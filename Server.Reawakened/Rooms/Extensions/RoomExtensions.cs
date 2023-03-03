using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Rooms.Extensions;

public static class RoomExtensions
{
    public static void SendSyncEvent(this Room room, SyncEvent syncEvent)
    {
        foreach (
            var client in
            from client in room.Clients.Values
            let receivedPlayer = client.Get<Player>()
            select client
        )
            client.SendSyncEventToPlayer(syncEvent);
    }
}
