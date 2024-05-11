using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss;

public class SpiderBossControllerComp : BaseAIStateMachine<SpiderBossController>, IRecieverTriggered, IDestructible
{
    /* 
     * -- AI STATES --
     * AIStateSpiderBase
     * AIStateSpiderDeactivated
     * [DONE] AIStateSpiderDrop
     * AIStateSpiderIdle
     * AIStateSpiderMove
     * AIStateSpiderPatrol
     * AIStateSpiderPhase1
     * AIStateSpiderPhase2
     * AIStateSpiderPhase3
     * AIStateSpiderPhaseTeaser
     * AIStateSpiderPhaseTrans
     * [DONE] AIStateSpiderRetreat
     * AIStateSpiderVenom
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
                TimerThread.DelayCall(RunEntrance, null, TimeSpan.FromSeconds(delay.Value), TimeSpan.Zero, 1);
        }
    }

    public void RunEntrance(object _)
    {
        if (Room == null)
            return;

        if (Teaser)
            AddNextState<AIStateSpiderTeaserEntranceComp>();
        else
            AddNextState<AIStateSpiderEntranceComp>();

        GoToNextState();
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
