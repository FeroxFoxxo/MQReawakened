using Server.Reawakened.Levels.Enums;

namespace Server.Reawakened.Levels.Models.Entities;

public interface ITriggerable
{
    public void TriggerStateChange(TriggerType triggerType, Level level, bool triggered);
}
