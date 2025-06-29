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

    public void PlayAnimOut() => Logger.LogTrace("PlayAnimOut called for {StateName} on {PrefabName}", StateName, PrefabName);
}
