using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollVacuumComp : BaseAIState<AIStateTrollVacuum, AI_State>
{
    public override string StateName => "AIStateTrollVacuum";

    public float CastingTime => ComponentData.CastingTime;
    public float Duration => ComponentData.Duration;
    public float CooldownTime => ComponentData.CooldownTime;
    public int AirFlowID => ComponentData.AirFlowID;

    public override AI_State GetInitialAIState() => new(
        [
            new (CastingTime, "Casting"),
            new (Duration, "Vacuum"),
            new (CooldownTime, "Cooldown")
        ], loop: false);

    public void Casting() => Logger.LogTrace("Casting called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Vacuum() => Logger.LogTrace("Vacuum called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Cooldown() => Logger.LogTrace("Cooldown called for {StateName} on {PrefabName}", StateName, PrefabName);
}
