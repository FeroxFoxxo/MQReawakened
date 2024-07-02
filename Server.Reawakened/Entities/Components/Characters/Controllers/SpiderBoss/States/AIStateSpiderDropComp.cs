using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;

public class AIStateSpiderDropComp : BaseAIState<AIStateSpiderDrop>
{
    public override string StateName => "AIStateSpiderDrop";

    public float GetUpDuration => ComponentData.GetUpDuration;
    public float FloorY => ComponentData.FloorY;
    public string[] SpawnerIds => ComponentData.SpawnerIds;
    public int[] NumberOfThrowPerPhase => ComponentData.NumberOfThrowPerPhase;

    public TimerThread TimerThread { get; set; }

    public override void StartState()
    {
        Position.SetPosition(Position.X, FloorY, Position.Z);

        TimerThread.DelayCall(RunIdleState, null, TimeSpan.FromSeconds(1), TimeSpan.Zero, 1);
    }

    public override ExtLevelEditor.ComponentSettings GetSettings() =>
        [Position.X.ToString(), FloorY.ToString(), Position.Z.ToString()];

    public void RunIdleState(object _)
    {
        if (Room == null)
            return;

        AddNextState<AIStateSpiderIdleComp>();

        GoToNextState();
    }
}
