using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Spiker.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiker.States;
public class AIStateSpikerAttackComp : BaseAIState<AIStateSpikerAttackMQR>
{
    public override string StateName => "AIStateSpikerAttack";

    public float ShootTime => ComponentData.ShootTime;
    public float ProjectileTime => ComponentData.ProjectileTime;
    public string Projectile => ComponentData.Projectile;
    public float ProjectileSpeed => ComponentData.ProjectileSpeed;
    public float FirstProjectileAngleOffset => ComponentData.FirstProjectileAngleOffset;
    public int NumberOfProjectiles => ComponentData.NumberOfProjectiles;
    public float AngleBetweenProjectiles => ComponentData.AngleBetweenProjectiles;

    // Provide ForceDirectionX
    public override ExtLevelEditor.ComponentSettings GetSettings() => throw new NotImplementedException();
}
