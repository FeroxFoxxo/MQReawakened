using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderPhase1Comp : BaseAIState<AIStateSpiderPhase1, AI_State>
{
    public override string StateName => "AIStateSpiderPhase1";

    public override AI_State GetInitialAIState() => new([], loop: false);

    public override void OnAIStateIn() => (StateMachine as SpiderBossControllerComp).CurrentPhase = 0;
}
