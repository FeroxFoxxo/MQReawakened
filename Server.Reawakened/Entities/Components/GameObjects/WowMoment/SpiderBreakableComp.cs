using Server.Reawakened.Entities.Components.GameObjects.Breakables.Abstractions;

namespace Server.Reawakened.Entities.Components.GameObjects.WowMoment;

public class SpiderBreakableComp : BaseBreakableObjStatusComp<SpiderBreakable>
{
    public float CamShakeIntensity => ComponentData.CamShakeIntensity;
    public float CamShakeFrequency => ComponentData.CamShakeFrequency;
    public float CamShakeDuration => ComponentData.CamShakeDuration;
}
