using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;

public class AIStateSpiderRetreatComp : BaseAIRetreatState<AIStateSpiderRetreat>
{
    public override string StateName => "AIStateSpiderRetreat";

    public float TransTime => ComponentData.TransTime;
    public float DieDuration => ComponentData.DieDuration;
    public float TalkDuration => ComponentData.TalkDuration;
    public int DoorToOpenID => ComponentData.DoorToOpenID;

    public override int DoorId => DoorToOpenID;
    public override float DelayUntilOpen => TransTime + TalkDuration + DieDuration;
}
