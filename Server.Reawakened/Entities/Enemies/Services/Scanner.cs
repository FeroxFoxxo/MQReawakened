using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Entities.Enemies.Services;
public class Scanner(BehaviorEnemy enemy) : IScan
{
    public override vector3 findTarget(float clockTime) =>
        FindClosestPosition();

    public override vector3 findClosestTarget(float radius) =>
        FindClosestPosition(radius);

    private vector3 FindClosestPosition(float radius = float.MaxValue)
    {
        var firstPlayer = enemy.Room.GetClosestPlayer(enemy.Position.ToUnityVector3(), radius);

        if (firstPlayer == null)
            return new vector3(0, 0, 0);

        var closestPos = firstPlayer.TempData.Position;

        return closestPos.ToVector3();
    }
}
