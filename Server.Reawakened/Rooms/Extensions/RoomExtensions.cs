using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System.Xml;
using UnityEngine;

namespace Server.Reawakened.Rooms.Extensions;

public static class RoomExtensions
{
    public static void SendSyncEvent(this Room room, SyncEvent syncEvent, Player sentPlayer = null)
    {
        foreach (
            var player in
            from player in room.GetPlayers()
            where sentPlayer == null || player.UserId != sentPlayer.UserId
            select player
        )
            player.SendSyncEventToPlayer(syncEvent);
    }

    public static Player GetClosetPlayer(this Room room, Vector3 currentPosition, float radius)
    {
        Player closestPlayer = null;
        var closestDistance = float.MaxValue;

        foreach (var player in room.GetPlayers())
        {
            if (player.Character.CurrentLife <= 0)
                continue;

            var distance = Vector3.Distance(player.TempData.Position, currentPosition);

            if (distance <= radius && distance <= closestDistance)
                closestPlayer = player;
        }

        return closestPlayer;
    }

    public static List<Player> GetNearbyPlayers(this Room room, Vector3 currentPosition, float radius)
    {
        var playersNearby = new List<Player>();

        foreach (var player in room.GetPlayers())
            if (Vector3.Distance(player.TempData.Position, currentPosition) <= radius)
                playersNearby.Add(player);

        return playersNearby;
    }

    public static bool IsPlayerNearby(this Room room, Vector3 currentPosition, float radius)
    {
        foreach (var player in room.GetPlayers())
            if (Vector3.Distance(player.TempData.Position, currentPosition) <= radius)
                return true;

        return false;
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

    public static bool IsOnBackPlane(this BaseComponent component, Microsoft.Extensions.Logging.ILogger logger)
    {
        if (component.ParentPlane == "Plane1")
            return true;
        else if (component.ParentPlane == "Plane0")
            return false;
        else
            logger.LogWarning("Unknown plane for portal: {PortalPlane}", component.ParentPlane);

        return false;
    }
}
