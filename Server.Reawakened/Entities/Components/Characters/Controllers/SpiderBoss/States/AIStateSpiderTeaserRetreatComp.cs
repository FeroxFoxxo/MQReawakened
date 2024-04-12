using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderTeaserRetreatComp : BaseAIRetreatState<AIStateSpiderTeaserRetreat>
{
    public override string StateName => "AIStateSpiderTeaserRetreat";

    public float TransTime => ComponentData.TransTime;
    public float DieDuration => ComponentData.DieDuration;
    public float TalkDuration => ComponentData.TalkDuration;
    public int DoorToOpenID => ComponentData.DoorToOpenID;

    public override int DoorId => DoorToOpenID;
    public override float DelayUntilOpen => TransTime + TalkDuration + DieDuration;
}
