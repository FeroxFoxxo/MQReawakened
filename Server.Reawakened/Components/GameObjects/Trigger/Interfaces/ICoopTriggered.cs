using Server.Reawakened.Entities.Enums;

namespace Server.Reawakened.Entities.Interfaces;

public interface ICoopTriggered
{
    public void TriggerStateChange(TriggerType triggerType, bool triggered);
}
