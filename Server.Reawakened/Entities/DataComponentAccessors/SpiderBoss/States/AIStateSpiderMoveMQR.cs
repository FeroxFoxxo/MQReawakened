using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.SpiderBoss.States;
public class AIStateSpiderMoveMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "AIStateSpiderMove";

    [MQConstant] public float[] MovementSpeed;
    [MQ] public float CeilingY;
    [MQ] public float PatrolFromY;
}
