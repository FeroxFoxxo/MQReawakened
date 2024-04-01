using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Enemies.AIStateEnemies.Rachnok.AIStates;
public class AIStateSpiderTeaserRetreatComp : Component<AIStateSpiderTeaserRetreat>
{
    public float TransTime => ComponentData.TransTime;
    public float DieDuration => ComponentData.DieDuration;
    public float TalkDuration => ComponentData.TalkDuration;
    public int DoorToOpenID => ComponentData.DoorToOpenID;
}
