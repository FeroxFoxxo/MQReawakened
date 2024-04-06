using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Drake.States;
public class AIStateDrakeAttackComp : Component<AIStateDrakeAttack>
{
    public float RamSpeed => ComponentData.RamSpeed;
    public float AttackOutAnimDuration => ComponentData.AttackOutAnimDuration;
    public float StunDuration => ComponentData.StunDuration;
    public float FleeSpeed => ComponentData.FleeSpeed;
    public float ReloadDuration => ComponentData.ReloadDuration;
    public float TauntAnimDuration => ComponentData.TauntAnimDuration;
}
