using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.Spiker.States;
public class AIStateSpikerAttackMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "AIStateSpikerAttack";

    [MQConstant] public float ShootTime;
    [MQConstant] public float ProjectileTime;
    [MQConstant] public string Projectile = "COL_PRJ_SpikeRock01";
    [MQConstant] public float ProjectileSpeed = 2f;
    [MQConstant] public float FirstProjectileAngleOffset;
    [MQConstant] public int NumberOfProjectiles = 3;
    [MQConstant] public float AngleBetweenProjectiles = 15f;
}
