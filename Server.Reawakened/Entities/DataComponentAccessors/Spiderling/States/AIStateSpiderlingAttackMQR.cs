using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;
public class AIStateSpiderlingAttackMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "AIStateSpiderlingAttack";

    [MQConstant] public float ShotInterval = 0.25f;
    [MQConstant] public float ShootTime = 2f;
    [MQConstant] public float ShootDelay = 1f;
    [MQConstant] public string Projectile = "COL_PRJ_Spiderling01";
    [MQConstant] public float ProjectileSpeed = 10f;
    [MQConstant] public float FirstProjectileAngleOffset = 35f;
    [MQConstant] public int NumberOfProjectiles = 1;
    [MQConstant] public float AngleBetweenProjectiles = -9f;
}
