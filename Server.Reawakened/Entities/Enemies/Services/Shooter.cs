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

    public override void spread(float clockTime, float speed, int count, float spreadAngle, vector3 inOrg, vector3 inTarg)
    {
        var velocities = new List<Vector3>();

        var origin = new Vector3(inOrg.x, inOrg.y, inOrg.z);
        var target = new Vector3(inTarg.x, inTarg.y, inTarg.z);

        var direction = (target - origin).normalized;
        var startRotation = Quaternion.LookRotation(direction);
        var startSpread = Quaternion.AngleAxis(-spreadAngle / 2, Vector3.up);

        for (var i = 0; i < count; i++)
        {
            var angleStep = spreadAngle / (count - 1);
            var projectileRotation = startRotation * startSpread * Quaternion.AngleAxis(angleStep * i, Vector3.up);
            var velocity = projectileRotation * Vector3.forward * speed;

            velocities.Add(velocity);
        }

        foreach (var velocity in velocities)
            enemy.FireProjectile(origin, velocity, false);

        LogFacade.debug("Sending experimental implementation of 'spread' in Shooter.cs class");
    }
}
