using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderPhase3Comp : BaseAIState<AIStateSpiderPhase3, AI_State>
{
    public override string StateName => "AIStateSpiderPhase3";

    public override AI_State GetInitialAIState() => new([], loop: false);

    public override void OnAIStateIn() => (StateMachine as SpiderBossControllerComp).CurrentPhase = 2;
}
