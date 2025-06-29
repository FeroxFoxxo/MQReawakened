using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollArmorBreakComp : BaseAIState<AIStateTrollArmorBreak, AI_State>
{
    public override string StateName => "AIStateTrollArmorBreak";

    public float StunTime => ComponentData.StunTime;
    public float RoarTime => ComponentData.RoarTime;

    public override AI_State GetInitialAIState() => new(
        [
            new (StunTime, "Stun"),
            new (0.6f, "Roar"),
            new (RoarTime - 1.2f, "RoarFX"),
            new (0.6f, "RoarFXDone")
        ], loop: false);

    public void Stun()
    {
        Logger.LogTrace("Stun called for {StateName} on {PrefabName}", StateName, PrefabName);

        Room.GetEntityFromId<IceTrollBossControllerComp>(Id).Broken = true;
    }

    public void Roar() => Logger.LogTrace("Roar called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void RoarFX() => Logger.LogTrace("RoarFX called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void RoarFXDone() => Logger.LogTrace("RoarFXDone called for {StateName} on {PrefabName}", StateName, PrefabName);
}
