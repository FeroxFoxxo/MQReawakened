using Server.Reawakened.Entities.Enums;

namespace Server.Reawakened.Rooms.Models.Entities;

public interface ICoopTriggered
{
    public void TriggerStateChange(TriggerType triggerType, bool triggered);
}
