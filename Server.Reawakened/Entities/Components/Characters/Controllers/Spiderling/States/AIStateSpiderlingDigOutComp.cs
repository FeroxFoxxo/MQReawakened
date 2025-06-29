using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling.States;
public class AIStateSpiderlingDigOutComp : BaseAIState<AIStateSpiderlingDigOutMQR, AI_State>
{
    public override string StateName => "AIStateSpiderlingDigOut";

    public bool DigOutOnSpawn => ComponentData.DigOutOnSpawn;
    public float AnimationDuration => ComponentData.AnimationDuration;

    public override AI_State GetInitialAIState() => new(
        [
            new ((!DigOutOnSpawn) ? 0f : AnimationDuration, "DigOut")
        ], loop: false);

    public override void Execute()
    {
        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);

        if (patrolComp == null)
            return;

        var closestPlayer = patrolComp.GetClosestPlayer();

        if (closestPlayer == null)
            return;

        DigOut();
    }

    public void DigOut()
    {
        Logger.LogTrace("Dig out called for {StateName} on {PrefabName}", StateName, PrefabName);

        AddNextState<AIStateSpiderlingAlertComp>();
        GoToNextState();
    }
}
