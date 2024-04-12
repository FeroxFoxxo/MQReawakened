using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.Base.States;
public class AIStateMoveMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "AIStateMove";

    [MQConstant] public float MovementSpeed;
}
