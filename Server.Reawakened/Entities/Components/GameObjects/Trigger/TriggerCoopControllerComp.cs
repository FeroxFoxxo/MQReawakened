using Server.Reawakened.Entities.Components.GameObjects.Trigger.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger;

public class TriggerCoopControllerComp : BaseTriggerCoopController<TriggerCoopController>
{
    public void SendTriggerEvent(Player player)
    {
        Room.SendSyncEvent(new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true));
        TriggerInteraction(ActivationType.NormalDamage, player);
        IsActive = true;
    }
}
