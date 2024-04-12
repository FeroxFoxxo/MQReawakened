using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.Base.States;
public class AIStateWaitMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "AIStateWait";

    [MQConstant] public float WaitDuration;
}
