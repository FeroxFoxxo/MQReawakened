using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderPhaseTransComp : BaseAIState<AIStateSpiderPhaseTrans, AI_State>
{
    public override string StateName => "AIStateSpiderPhaseTrans";

    public float InTime => ComponentData.InTime;
    public float AirInTime => ComponentData.AirInTime;
    public float LoopTime => ComponentData.LoopTime;
    public float OutTime => ComponentData.OutTime;

    public override AI_State GetInitialAIState() => new(
        [
            new (InTime, "PlayAnimIn"),
            new (LoopTime, "PlayAnimLoop"),
            new (OutTime, "PlayAnimOut")
        ], loop: false);

    public override void OnAIStateIn()
    {
        if (!(StateMachine as SpiderBossControllerComp).OnGround)
            State.SetTime("PlayAnimIn", AirInTime);
        else
            State.SetTime("PlayAnimIn", InTime);

        State.RecalculateTimes();
    }

    public void PlayAnimIn() => Logger.LogTrace("PlayAnimIn called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void PlayAnimLoop() => Logger.LogTrace("PlayAnimLoop called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void PlayAnimOut()
    {
        Logger.LogTrace("PlayAnimOut called for {StateName} on {PrefabName}", StateName, PrefabName);

        if (StateMachine is not SpiderBossControllerComp controller)
            return;

        if (controller.EnemyData == null || controller.EnemyData.MaxHealth <= 0)
            return;

        var ratio = (float)controller.EnemyData.Health / controller.EnemyData.MaxHealth;

        if (controller.Phase02Trans > 0 && ratio <= controller.Phase02Trans)
            AddNextState<AIStateSpiderPhase3Comp>();
        else if (controller.Phase01Trans > 0 && ratio <= controller.Phase01Trans)
            AddNextState<AIStateSpiderPhase2Comp>();

        AddNextState<AIStateSpiderIdleComp>();
        GoToNextState();
    }
}
