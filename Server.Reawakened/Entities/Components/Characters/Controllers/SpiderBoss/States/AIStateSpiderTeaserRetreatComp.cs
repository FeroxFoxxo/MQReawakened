namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderTeaserRetreatComp : BaseAIState<AIStateSpiderTeaserRetreat>
{
    public float TransTime => ComponentData.TransTime;
    public float DieDuration => ComponentData.DieDuration;
    public float TalkDuration => ComponentData.TalkDuration;
    public int DoorToOpenID => ComponentData.DoorToOpenID;
}
