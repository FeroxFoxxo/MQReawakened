using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss;

public class SpiderBossControllerComp : BaseAIStateMachine<SpiderBossController>, IRecieverTriggered, IDestructible
{
    /* 
     * -- AI STATES --
     * AIStateSpiderBase
     * AIStateSpiderDeactivated
     * [DONE] AIStateSpiderDrop
     * [DONE] AIStateSpiderIdle
     * AIStateSpiderMove
     * AIStateSpiderPatrol
     * AIStateSpiderPhase1
     * AIStateSpiderPhase2
     * AIStateSpiderPhase3
     * AIStateSpiderPhaseTeaser
     * AIStateSpiderPhaseTrans
     * [DONE] AIStateSpiderRetreat
     * [DONE] AIStateSpiderVenom
     * AIStateSpiderVineThrow
     * AIStateSpiderWebs
     * 
     * -- BOSS ONLY --
     * [DONE] AIStateSpiderEntrance
     * AIStateSpiderSwichSide
     * 
     * -- TEASER ONLY --
     * [DONE] AIStateSpiderTeaserEntrance
     * [DONE] AIStateSpiderTeaserRetreat
    */

    public bool Teaser => ComponentData.Teaser;
    public string NPCId => ComponentData.NPCId;
    public string NPCTriggerId => ComponentData.NPCTriggerId;

    public TimerThread TimerThread { get; set; }

    public void RecievedTrigger(bool triggered)
    {
        if (Room == null)
            return;

        if (triggered)
        {
            var delay = Teaser ?
                Room.GetEntityFromId<AIStateSpiderTeaserEntranceComp>(Id)?.DelayBeforeEntranceDuration :
                Room.GetEntityFromId<AIStateSpiderEntranceComp>(Id)?.DelayBeforeEntranceDuration;

            if (delay.HasValue)
                TimerThread.RunDelayed(RunEntrance, this, TimeSpan.FromSeconds(delay.Value));
        }
    }

    public static void RunEntrance(ITimerData data)
    {
        if (data is not SpiderBossControllerComp spider)
            return;

        if (spider.Teaser)
            spider.AddNextState<AIStateSpiderTeaserEntranceComp>();
        else
            spider.AddNextState<AIStateSpiderEntranceComp>();

        spider.GoToNextState();
    }

    public void Destroy(Room room, string id)
    {
        if (room == null)
            return;

        if (Teaser)
            AddNextState<AIStateSpiderTeaserRetreatComp>();
        else
            AddNextState<AIStateSpiderRetreatComp>();

        GoToNextState();
    }
}
