using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderPhaseTransComp : BaseAIState<AIStateSpiderPhaseTrans>
{
    public override string StateName => "AIStateSpiderPhaseTrans";

    public float InTime => ComponentData.InTime;
    public float AirInTime => ComponentData.AirInTime;
    public float LoopTime => ComponentData.LoopTime;
    public float OutTime => ComponentData.OutTime;
}
