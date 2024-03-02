using Server.Reawakened.Entities.AbstractComponents;

namespace Server.Reawakened.Entities.Components;

public class SpiderBreakableComp : BaseBreakableObjStatusComp<SpiderBreakable>
{
    public float CamShakeIntensity => ComponentData.CamShakeIntensity;
    public float CamShakeFrequency => ComponentData.CamShakeFrequency;
    public float CamShakeDuration => ComponentData.CamShakeDuration;
}
