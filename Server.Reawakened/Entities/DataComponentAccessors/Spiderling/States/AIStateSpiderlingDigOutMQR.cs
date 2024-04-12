using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;
public class AIStateSpiderlingDigOutMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "AIStateSpiderlingDigOut";

    [MQ] public bool DigOutOnSpawn;
    [MQConstant] public float AnimationDuration = 1f;
}
