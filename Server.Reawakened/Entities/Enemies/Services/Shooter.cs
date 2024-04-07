using Server.Reawakened.Entities.Enemies.EnemyTypes;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.Services;
public class Shooter(BehaviorEnemy enemy) : IShoot
{
    public override int projectile(float clockTime, float speedX, float speedY, float posX, float posY, float posZ, bool isLob)
    {
        LogFacade.error("Running unimplemented AI method 'projectile (x,y,z)' (from Shooter.cs)");
        return 0;
    }

    public override int projectile(float clockTime, float projSpeed, float maxHeight, vector3 intPos, vector3 intTarget)
    {
        var pos = new Vector3(intPos.x, intPos.y, intPos.z);
        var target = new Vector3(intTarget.x, intTarget.y, intTarget.z);
        var gravity = enemy.ServerRConfig.Gravity;

        var distance = target.x - pos.x;
        var heightDifference = target.y - pos.y;
        var peak = Math.Max(heightDifference, maxHeight);

        var timeToPeak = Convert.ToSingle(Math.Sqrt(2 * peak / gravity));
        var totalTime = 2 * timeToPeak;

        var vx = distance / totalTime;
        var vy = gravity * timeToPeak;

        var position = new Vector3(pos.x, pos.y, pos.z);
        var velocity = new Vector2(vx, vy);

        enemy.FireProjectile(position, velocity, true);

        return 0;
    }

    public override void spread(float clockTime, float speed, int count, float spreadAngle, vector3 origin, vector3 target) =>
        LogFacade.error("Running unimplemented AI method 'spread' (from Shooter.cs)");
}
