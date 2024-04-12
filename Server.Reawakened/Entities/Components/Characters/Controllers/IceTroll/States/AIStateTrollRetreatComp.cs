using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollRetreatComp : BaseAIRetreatState<AIStateTrollRetreat>
{
    public override string StateName => "AIStateTrollRetreat";

    public float TransTime => ComponentData.TransTime;
    public float TalkDuration => ComponentData.TalkDuration;
    public float DieDuration => ComponentData.DieDuration;
    public int DoorToOpenID => ComponentData.DoorToOpenID;

    public override int DoorId => DoorToOpenID;

    public override float DelayUntilOpen => TransTime + TalkDuration + DieDuration;
}
