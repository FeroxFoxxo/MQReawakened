using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;

public interface ICoopTriggered
{
    void TriggerStateChange(TriggerType triggerType, bool triggered, string triggeredBy);
}
