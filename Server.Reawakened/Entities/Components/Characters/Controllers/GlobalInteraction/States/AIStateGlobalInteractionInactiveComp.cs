using GlobalInteractionEvents;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.GlobalInteraction.States;
public class AIStateGlobalInteractionInactiveComp : BaseAIState<AIStateGlobalInteractionInactive, AI_State>
{
    public override string StateName => "AIStateGlobalInteractionInactive";

    private readonly GlobalInteractionEventState _globalInteractionEventState = new();

    public override AI_State GetInitialAIState() => new([], loop: true);

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        GlobalInteractionController.PercentageOfRequiredInteractions = 0;
    }

    public override void OnAIStateIn()
    {
        Logger.LogTrace("OnAIStateIn called for {StateName} on {PrefabName}", StateName, PrefabName);

        _globalInteractionEventState.CurrentState = GlobalInteractionStateMachineState.InActive;
        _globalInteractionEventState.CurrentInteractionStatus = CurrentGlobalInteractionStateStatus.In;
    }

    public override void Execute()
    {
        _globalInteractionEventState.CurrentState = GlobalInteractionStateMachineState.InActive;
        _globalInteractionEventState.CurrentInteractionStatus = CurrentGlobalInteractionStateStatus.Execute;
    }

    public override void OnAIStateOut()
    {
        Logger.LogTrace("OnAIStateOut called for {StateName} on {PrefabName}", StateName, PrefabName);

        _globalInteractionEventState.CurrentState = GlobalInteractionStateMachineState.InActive;
        _globalInteractionEventState.CurrentInteractionStatus = CurrentGlobalInteractionStateStatus.Out;
    }
}
