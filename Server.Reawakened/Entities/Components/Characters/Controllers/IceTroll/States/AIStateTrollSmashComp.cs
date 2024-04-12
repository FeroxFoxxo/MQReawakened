using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollSmashComp : BaseAIState<AIStateTrollSmash>
{
    public override string StateName => "AIStateTrollSmash";

    public float ChargingAttackDuration => ComponentData.ChargingAttackDuration;
    public float DoingAttackDuration => ComponentData.DoingAttackDuration;
    public float ImpactAttackDuration => ComponentData.ImpactAttackDuration;
    public float CooldownDuration => ComponentData.CooldownDuration;
    public float DamageRadius => ComponentData.DamageRadius;
    public float DamageOffset => ComponentData.DamageOffset;
}
