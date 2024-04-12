using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Drake.States;
public class AIStateDrakeAttackComp : BaseAIState<AIStateDrakeAttack>
{
    public override string StateName => "AIStateDrakeAttack";

    public float RamSpeed => ComponentData.RamSpeed;
    public float AttackOutAnimDuration => ComponentData.AttackOutAnimDuration;
    public float StunDuration => ComponentData.StunDuration;
    public float FleeSpeed => ComponentData.FleeSpeed;
    public float ReloadDuration => ComponentData.ReloadDuration;
    public float TauntAnimDuration => ComponentData.TauntAnimDuration;

    // Provide Initial, Placement And Target Positions
    public override ExtLevelEditor.ComponentSettings GetSettings() => throw new NotImplementedException();
}
