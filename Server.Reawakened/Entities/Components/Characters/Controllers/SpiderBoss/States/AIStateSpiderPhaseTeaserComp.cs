using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderPhaseTeaserComp : BaseAIState<AIStateSpiderPhaseTeaser, AI_State>
{
    public override string StateName => "AIStateSpiderPhaseTeaser";

    public override AI_State GetInitialAIState() => new([], loop: false);

    public override void OnAIStateIn() => (StateMachine as SpiderBossControllerComp).CurrentPhase = 0;
}
