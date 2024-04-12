using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;
public class AIStateSpiderlingAlertMQR : DataComponentAccessorMQR
{
    public override string OverrideName => "AIStateSpiderlingAlert";

    [MQ] public float AlertTime = 1f;
}
