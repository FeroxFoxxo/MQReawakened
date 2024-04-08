using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.Services;
public class Scanner(BehaviorEnemy enemy) : IScan
{
    public override vector3 findTarget(float clockTime) =>
        FindClosestPosition(enemy.Room.GetPlayers());

    public override vector3 findClosestTarget(float radius) =>
        FindClosestPosition(enemy.Room.GetNearbyPlayers(enemy.Position, radius));

    private vector3 FindClosestPosition(IEnumerable<Player> players)
    {
        var firstPlayer = players.FirstOrDefault(x => x != null);

        if (firstPlayer == null)
            return new vector3(0, 0, 0);

        var closestDistance = float.MaxValue;
        var closestPos = firstPlayer.TempData.Position;

        foreach (var player in players)
        {
            var distance = Vector3.Distance(player.TempData.Position, enemy.Position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPos = player.TempData.Position;
            }
        }

        return new vector3(closestPos.x, closestPos.y, closestPos.z);
    }
}
