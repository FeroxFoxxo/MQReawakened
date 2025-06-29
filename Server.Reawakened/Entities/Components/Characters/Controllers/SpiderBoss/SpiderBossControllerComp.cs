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

    public int CurrentPhase = 0;
    public bool OnGround = true;

    public void RecievedTrigger(bool triggered)
    {
        if (Room == null)
            return;

        if (triggered)
        {
            if (Teaser)
                AddNextState<AIStateSpiderTeaserEntranceComp>();
            else
                AddNextState<AIStateSpiderEntranceComp>();

            GoToNextState();
        }
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
