using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderWebsComp : BaseAIState<AIStateSpiderWebs>
{
    public override string StateName => "AIStateSpiderWebs";

    public float WebInTime => ComponentData.WebInTime;
    public float WebShootTime => ComponentData.WebShootTime;
    public float WebOutTime => ComponentData.WebOutTime;
    public int[] NumberOfShotPerPhase => ComponentData.NumberOfShotPerPhase;
    public string ProjectilePrefabName => ComponentData.ProjectilePrefabName;
    public string OnProjectileDestroyedPrefabCreation => ComponentData.OnProjectileDestroyedPrefabCreation;
    public float ProjectileSpeedY => ComponentData.ProjectileSpeedY;
    public float ProjectileSpeedMaxX => ComponentData.ProjectileSpeedMaxX;
}
