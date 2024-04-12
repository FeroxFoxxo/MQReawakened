using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.Hampster.States;
public class AIStateHamsterAttackMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "AIStateHamsterAttack";

    [MQConstant] public float InTime;
    [MQConstant] public float LoopTime;
    [MQConstant] public float OutTime;
    [MQConstant] public float JumpHeight = 2f;
}
