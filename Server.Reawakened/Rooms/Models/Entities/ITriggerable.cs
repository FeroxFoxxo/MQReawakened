using Server.Reawakened.Rooms.Enums;

namespace Server.Reawakened.Rooms.Models.Entities;

public interface ITriggerable
{
    public void TriggerStateChange(TriggerType triggerType, Room room, bool triggered);
}
