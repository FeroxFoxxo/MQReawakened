using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderVineThrowComp : BaseAIState<AIStateSpiderVineThrow, AI_State>
{
    public override string StateName => "AIStateSpiderVineThrow";

    public float AnimationInTime => ComponentData.AnimationInTime;
    public float VineThrowTime => ComponentData.VineThrowTime;
    public float AnimationOutTime => ComponentData.AnimationOutTime;

    public override AI_State GetInitialAIState() => new (
        [
            new (AnimationInTime, "Rise"),
            new (VineThrowTime, "ShotVine"),
            new (AnimationOutTime, "Finish")
        ], loop: false);


    public void Rise() => Logger.LogTrace("Rise called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void ShotVine() => Logger.LogTrace("ShotVine called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Finish() => Logger.LogTrace("Finish called for {StateName} on {PrefabName}", StateName, PrefabName);
}
