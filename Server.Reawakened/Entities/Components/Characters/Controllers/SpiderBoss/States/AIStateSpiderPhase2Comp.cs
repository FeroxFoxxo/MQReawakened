using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderPhase2Comp : BaseAIState<AIStateSpiderPhase2, AI_State>
{
    public override string StateName => "AIStateSpiderPhase2";

    public override AI_State GetInitialAIState() => new([], loop: false);

    public override void OnAIStateIn() => (StateMachine as SpiderBossControllerComp).CurrentPhase = 1;
}
