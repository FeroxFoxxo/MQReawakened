using Server.Reawakened.Entities.Enemies.EnemyTypes;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.Services;
public class Shooter(BehaviorEnemy enemy) : IShoot
{
    public override int projectile(float clockTime, float speedX, float speedY, float posX, float posY, float posZ, bool isLob)
    {
        var position = new Vector3(posX, posY, posZ);
        var velocity = new Vector2(speedX, speedY);

        enemy.FireProjectile(position, velocity, isLob);

        return 0;
    }

    public override int projectile(float clockTime, float projSpeed, float maxHeight, vector3 intPos, vector3 intTarget)
    {
        var pos = new Vector3(intPos.x, intPos.y, intPos.z);
        var target = new Vector3(intTarget.x, intTarget.y, intTarget.z);
        var gravity = enemy.ServerRConfig.Gravity;

        var distance = target.x - pos.x;
        var deltaY = target.y - pos.y;

        var timeToPeak = Math.Sqrt(2 * maxHeight / gravity);
        var totalTime = timeToPeak + Math.Sqrt(2 * (maxHeight + deltaY) / gravity);

        var vx = Convert.ToSingle(distance / totalTime);
        var vy = Convert.ToSingle(gravity * timeToPeak);

        var position = new Vector3(pos.x, pos.y, pos.z);
        var velocity = new Vector2(vx, vy);

        enemy.FireProjectile(position, velocity, true);

        return 0;
    }

    public override void spread(float clockTime, float speed, int count, float spreadAngle, vector3 origin, vector3 target) =>
        LogFacade.error("Running unimplemented AI method 'spread' (from Shooter.cs)");
}
