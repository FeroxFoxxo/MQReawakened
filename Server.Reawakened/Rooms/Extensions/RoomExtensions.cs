using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using System.Xml;

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

    public static string GetValue(this XmlAttributeCollection attributes, string valName)
    {
        var v = attributes.GetNamedItem(valName);
        return v == null ? string.Empty : v.Value ?? string.Empty;
    }

    public static int GetIntValue(this XmlAttributeCollection attributes, string valName)
    {
        var x = attributes.GetValue(valName);
        return !string.IsNullOrEmpty(x) ? Convert.ToInt32(x) : 0;
    }

    public static float GetSingleValue(this XmlAttributeCollection attributes, string valName)
    {
        var x = attributes.GetValue(valName);
        return !string.IsNullOrEmpty(x) ? Convert.ToSingle(x) : 0;
    }
}
