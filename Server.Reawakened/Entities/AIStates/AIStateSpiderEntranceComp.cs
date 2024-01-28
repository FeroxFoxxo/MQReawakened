using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.AIStates;

public class AIStateSpiderEntranceComp : Component<AIStateSpiderEntrance>
{
    public float DelayBeforeEntranceDuration => ComponentData.DelayBeforeEntranceDuration;
    public float EntranceDuration => ComponentData.EntranceDuration;
    public float IntroDuration => ComponentData.IntroDuration;
}
