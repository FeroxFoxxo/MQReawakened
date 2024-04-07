using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Rooms.Extensions;
using System;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.Services;
public class Scanner(BehaviorEnemy enemy) : IScan
{
    public override vector3 findTarget(float clockTime)
    {
        LogFacade.error("Running unimplemented AI method 'findTarget' (from Scanner.cs)");
        return findClosestTarget(10f); // Temporary
    }

    public override vector3 findClosestTarget(float radius)
    {
        var nearestPlayers = enemy.Room.GetNearbyPlayers(enemy.Position, radius);

        if (nearestPlayers.Count == 0)
            return new vector3(0, 0, 0);

        var closestDistance = float.MaxValue;
        var closestPos = nearestPlayers.First().TempData.Position;

        foreach (var player in nearestPlayers)
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
