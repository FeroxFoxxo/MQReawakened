using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Drake.States;
public class AIStateDrakeAttackComp : BaseAIState<AIStateDrakeAttack>
{
    public float RamSpeed => ComponentData.RamSpeed;
    public float AttackOutAnimDuration => ComponentData.AttackOutAnimDuration;
    public float StunDuration => ComponentData.StunDuration;
    public float FleeSpeed => ComponentData.FleeSpeed;
    public float ReloadDuration => ComponentData.ReloadDuration;
    public float TauntAnimDuration => ComponentData.TauntAnimDuration;
}
