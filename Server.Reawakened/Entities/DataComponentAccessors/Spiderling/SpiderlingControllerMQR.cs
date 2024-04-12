using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.Spiderling;
public class SpiderlingControllerMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "SpiderlingController";

    [MQ] public bool StartIdle = true;
    [MQConstant] public float TimeToDirtFXInTaunt;
}
