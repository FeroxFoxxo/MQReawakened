using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollPhase3Comp : BaseAIState<AIStateTrollPhase3, AI_State_TrollBase>
{
    public override string StateName => "AIStateTrollPhase3";

    public override AI_State_TrollBase GetInitialAIState() => new();

    public override void OnAIStateIn() => (StateMachine as IceTrollBossControllerComp).CurrentPhase = 3;
}
