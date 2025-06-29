using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollBreathComp : BaseAIState<AIStateTrollBreath, AI_State>
{
    public override string StateName => "AIStateTrollBreath";

    public float CastingTime => ComponentData.CastingTime;
    public float Duration => ComponentData.Duration;
    public float CooldownTime => ComponentData.CooldownTime;
    public int AirFlowID => ComponentData.AirFlowID;

    public override AI_State GetInitialAIState() => new (
        [
            new (CastingTime, "Casting"),
            new (Duration, "Breath"),
            new (CooldownTime, "Cooldown")
        ], loop: false);

    public void Casting() => Logger.LogTrace("Casting called for {StateName} on {PrefabName}", StateName, PrefabName);
    public void Breath() => Logger.LogTrace("Breath called for {StateName} on {PrefabName}", StateName, PrefabName);
    public void Cooldown() => Logger.LogTrace("Cooldown called for {StateName} on {PrefabName}", StateName, PrefabName);
}
