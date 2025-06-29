using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollSmashComp : BaseAIState<AIStateTrollSmash, AI_State>
{
    public override string StateName => "AIStateTrollSmash";

    public float ChargingAttackDuration => ComponentData.ChargingAttackDuration;
    public float DoingAttackDuration => ComponentData.DoingAttackDuration;
    public float ImpactAttackDuration => ComponentData.ImpactAttackDuration;
    public float CooldownDuration => ComponentData.CooldownDuration;
    public float DamageRadius => ComponentData.DamageRadius;
    public float DamageOffset => ComponentData.DamageOffset;

    public override AI_State GetInitialAIState() => new(
        [
            new AIDataEvent(ChargingAttackDuration, "ChargingAttack"),
            new AIDataEvent(DoingAttackDuration, "DoingAttack"),
            new AIDataEvent(ImpactAttackDuration, "ImpactAttack"),
            new AIDataEvent(CooldownDuration, "Cooldown")
        ], loop: false);

    public void ChargingAttack() => Logger.LogTrace("ChargingAttack called for {StateName} on {PrefabName}", StateName, PrefabName);
    public void DoingAttack() => Logger.LogTrace("DoingAttack called for {StateName} on {PrefabName}", StateName, PrefabName);
    public void ImpactAttack() => Logger.LogTrace("ImpactAttack called for {StateName} on {PrefabName}", StateName, PrefabName);
    public void Cooldown() => Logger.LogTrace("Cooldown called for {StateName} on {PrefabName}", StateName, PrefabName);
}
