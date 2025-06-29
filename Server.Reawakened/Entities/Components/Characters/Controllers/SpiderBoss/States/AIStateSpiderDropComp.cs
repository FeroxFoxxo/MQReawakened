using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;

public class AIStateSpiderDropComp : BaseAIState<AIStateSpiderDrop, AI_State_Drop>
{
    public override string StateName => "AIStateSpiderDrop";

    public float GetUpDuration => ComponentData.GetUpDuration;
    public float FloorY => ComponentData.FloorY;
    public string[] SpawnerIds => ComponentData.SpawnerIds;
    public int[] NumberOfThrowPerPhase => ComponentData.NumberOfThrowPerPhase;

    private readonly vector3 _dampingVelocity = new (0f, 0f, 0f);

    public override AI_State_Drop GetInitialAIState() => new(
        [
            new (5f, "Dropping"),
            new (GetUpDuration, "Dropped")
        ], FloorY);

    public override ExtLevelEditor.ComponentSettings GetSettings() =>
        [Position.X.ToString(), FloorY.ToString(), Position.Z.ToString()];

    public override void Execute()
    {
        var position = Position.ToVector3();
        var currentPosition = State.CurrentPosition;
        var dampenedPosition = new vector3(0f, 0f, 0f);
        var springK = 200f;
        MathUtils.CriticallyDampedSpring1D(springK, position.x, currentPosition.x, ref _dampingVelocity.x, ref dampenedPosition.x, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, position.y, currentPosition.y, ref _dampingVelocity.y, ref dampenedPosition.y, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, position.z, currentPosition.z, ref _dampingVelocity.z, ref dampenedPosition.z, Room.DeltaTime);
        Position.SetPosition(dampenedPosition);
    }

    public void Dropping() => Logger.LogTrace("Dropping called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Dropped()
    {
        Logger.LogTrace("Dropped called for {StateName} on {PrefabName}", StateName, PrefabName);

        Room.GetEntityFromId<SpiderBossControllerComp>(Id).OnGround = true;

        RunIdleState();
    }

    public void RunIdleState()
    {
        AddNextState<AIStateSpiderIdleComp>();
        GoToNextState();
    }
}
