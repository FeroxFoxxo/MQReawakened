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

    public override int projectile(float clockTime, float projSpeed, float maxHeight, vector3 pos, vector3 target)
    {
        var totalTime = (float)Math.Sqrt(2 * maxHeight / enemy.ServerRConfig.Gravity);

        var speed = new Vector2((target.x - pos.x) / totalTime, enemy.ServerRConfig.Gravity * totalTime);
        var position = new Vector3(pos.x, pos.y, pos.z);

        enemy.FireProjectile(position, speed, true);

        return 0;
    }

    public override void spread(float clockTime, float speed, int count, float spreadAngle, vector3 origin, vector3 target) =>
        LogFacade.error("Running unimplemented AI method 'spread' (from Shooter.cs)");
}
