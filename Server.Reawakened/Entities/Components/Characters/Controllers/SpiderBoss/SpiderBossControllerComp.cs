using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss;

public class SpiderBossControllerComp : BaseAIStateMachine<SpiderBossController>, IRecieverTriggered, IDestructible, IAIDamageEnemy
{
    public bool Teaser => ComponentData.Teaser;
    public string NPCId => ComponentData.NPCId;
    public string NPCTriggerId => ComponentData.NPCTriggerId;

    public int CurrentPhase = 0;
    public bool OnGround = true;
    private float _teaserStartTime = -1f;

    public float Phase01Trans { get; private set; }
    public float Phase02Trans { get; private set; }
    public float TeaserEndLifeRatio { get; private set; }
    public float TeaserEndTimeLimit { get; private set; }

    public bool IsRightSide = false;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        var baseComp = Room.GetEntityFromId<AIStateSpiderBaseComp>(Id);

        Phase01Trans = baseComp.HealthRatioPhase01Trans;
        Phase02Trans = baseComp.HealthRatioPhase02Trans;
        TeaserEndLifeRatio = baseComp.TeaserEndLifeRatio;
        TeaserEndTimeLimit = baseComp.TeaserEndTimeLimit;

        var switchComp = Room.GetEntityFromId<AIStateSpiderSwichSideComp>(Id);

        IsRightSide = switchComp.StartRight;

        Room.AddUpdatingKilledEnemy(Id);
    }

    public void RecievedTrigger(bool triggered)
    {
        if (Room == null)
            return;

        if (triggered)
        {
            if (Teaser)
            {
                Logger.LogTrace("SpiderBoss trigger received. Teaser={Teaser}", Teaser);

                Logger.LogTrace("Queued states: PhaseTeaser -> TeaserEntrance");

                AddNextState<AIStateSpiderPhaseTeaserComp>();
                AddNextState<AIStateSpiderTeaserEntranceComp>();
            }
            else
            {
                Logger.LogTrace("Queued states: Phase1 -> Entrance");

                AddNextState<AIStateSpiderPhase1Comp>();
                AddNextState<AIStateSpiderEntranceComp>();
            }

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

    public void EnemyDamaged(bool isDead)
    {
        if (isDead || Room == null || Teaser)
            return;

        if (CurrentStates.Any(s => s is AIStateSpiderPhaseTransComp))
            return;

        if (EnemyData == null || EnemyData.MaxHealth <= 0)
            return;

        var ratio = (float)EnemyData.Health / EnemyData.MaxHealth;
        var target = GetTargetPhase(ratio, Phase01Trans, Phase02Trans);
        Logger.LogTrace("Damage event: ratio={Ratio:F2}, currentPhase={Current}, targetPhase={Target}", ratio, CurrentPhase, target);

        if (target > CurrentPhase)
        {
            Logger.LogTrace("Enqueue PhaseTrans (current={Current} -> target={Target})", CurrentPhase, target);

            AddNextState<AIStateSpiderPhaseTransComp>();
            GoToNextState();
        }
    }

    private static int GetTargetPhase(float healthRatio, float trans1, float trans2) =>
        (trans2 > 0 && healthRatio <= trans2) ? 2 :
        (trans1 > 0 && healthRatio <= trans1) ? 1 : 0;

    public override void Update()
    {
        if (Room == null)
            return;

        var shouldTeaserRetreat = Teaser && ShouldTriggerTeaserRetreat();

        base.Update();

        if (shouldTeaserRetreat)
        {
            Logger.LogTrace("Teaser retreat queued after Update");

            AddNextState<AIStateSpiderTeaserRetreatComp>();
            GoToNextState();
        }
    }

    private bool ShouldTriggerTeaserRetreat()
    {
        if (CurrentStates.Any(s => s is AIStateSpiderTeaserRetreatComp))
            return false;

        if (_teaserStartTime >= 0 && TeaserEndTimeLimit > 0 && Room.Time - _teaserStartTime >= TeaserEndTimeLimit)
        {
            Logger.LogTrace("Teaser time limit reached");
            return true;
        }

        if (EnemyData != null && EnemyData.MaxHealth > 0)
        {
            var ratio = (float)EnemyData.Health / EnemyData.MaxHealth;

            if (TeaserEndLifeRatio > 0 && TeaserEndLifeRatio < 1f && ratio < TeaserEndLifeRatio && ratio < 0.9999f)
            {
                Logger.LogTrace("Teaser health threshold met (ratio={Ratio:F3} < {Thresh:F3})", ratio, TeaserEndLifeRatio);
                return true;
            }
        }

        return false;
    }

    public void MarkTeaserFightStart()
    {
        if (!Teaser || Room == null)
            return;

        if (_teaserStartTime < 0f)
        {
            _teaserStartTime = Room.Time;
            Logger.LogTrace("Teaser fight timer started (t={Time:F2})", _teaserStartTime);
        }
    }
}
