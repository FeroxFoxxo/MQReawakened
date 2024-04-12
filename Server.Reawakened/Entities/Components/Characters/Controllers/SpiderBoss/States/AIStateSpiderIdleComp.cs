using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderIdleComp : BaseAIState<AIStateSpiderIdle>
{
    public override string StateName => "AIStateSpiderIdle";

    public float[] Durations => ComponentData.Durations;
}
