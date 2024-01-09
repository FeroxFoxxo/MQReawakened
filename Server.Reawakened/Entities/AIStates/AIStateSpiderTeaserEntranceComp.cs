using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.AIStates;

public class AIStateSpiderTeaserEntranceComp : Component<AIStateSpiderTeaserEntrance>
{
    public float DelayBeforeEntranceDuration => ComponentData.DelayBeforeEntranceDuration;
    public float EntranceDuration => ComponentData.EntranceDuration;
    public float IntroDuration => ComponentData.IntroDuration;
    public float MinimumLifeRatioAccepted => ComponentData.MinimumLifeRatioAccepted;
    public float LifeRatioAtHeal => ComponentData.LifeRatioAtHeal;
}
