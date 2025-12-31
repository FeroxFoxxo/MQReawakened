using Server.Reawakened.Entities.Components.GameObjects.Breakables.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;

namespace Server.Reawakened.Entities.Components.GameObjects.WowMoment;

public class SpiderBreakableComp : BaseBreakableObjStatusComp<SpiderBreakable>, IBreakable
{
    public float CamShakeIntensity => ComponentData.CamShakeIntensity;
    public float CamShakeFrequency => ComponentData.CamShakeFrequency;
    public float CamShakeDuration => ComponentData.CamShakeDuration;
}
