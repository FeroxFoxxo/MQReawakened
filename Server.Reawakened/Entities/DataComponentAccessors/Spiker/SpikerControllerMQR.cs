using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.Spiker;
public class SpikerControllerMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "SpikerController";

    [MQ] public bool StartIdle = true;
    [MQConstant] public float TimeToDirtFXInTaunt;
}
