using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderVineThrowComp : BaseAIState<AIStateSpiderVineThrow>
{
    public override string StateName => "AIStateSpiderVineThrow";

    public float AnimationInTime => ComponentData.AnimationInTime;
    public float VineThrowTime => ComponentData.VineThrowTime;
    public float AnimationOutTime => ComponentData.AnimationOutTime;
}
