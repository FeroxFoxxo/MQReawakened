using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.Hampster;
public class HamsterControllerMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "HamsterController";

    [MQ] public bool StartIdle = true;
    [MQConstant] public float TimeToDirtFXInStunOut;
}
