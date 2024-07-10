using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;

public interface ICoopTriggered
{
    public void TriggerStateChange(TriggerType triggerType, bool triggered, string triggeredBy);
}
