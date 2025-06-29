using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling.States;
public class AIStateSpiderlingDigOutComp : BaseAIState<AIStateSpiderlingDigOutMQR>
{
    public override string StateName => "AIStateSpiderlingDigOut";

    public bool DigOutOnSpawn => ComponentData.DigOutOnSpawn;
    public float AnimationDuration => ComponentData.AnimationDuration;

    public override void UpdateState()
    {
        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);

        if (patrolComp == null)
            return;

        var closestPlayer = patrolComp.GetClosestPlayer();

        if (closestPlayer == null)
            return;

        AddNextState<AIStateSpiderlingAlertComp>();
        GoToNextState();
    }
}
