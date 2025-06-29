using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollPhase1Comp : BaseAIState<AIStateTrollPhase1, AI_State_TrollBase>
{
    public override string StateName => "AIStateTrollPhase1";

    public override AI_State_TrollBase GetInitialAIState() => new();

    public override void OnAIStateIn() => (StateMachine as IceTrollBossControllerComp).CurrentPhase = 1;
}
