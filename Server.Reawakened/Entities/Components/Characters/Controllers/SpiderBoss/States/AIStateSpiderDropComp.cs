using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;

public class AIStateSpiderDropComp : BaseAIState<AIStateSpiderDrop>
{
    public override string StateName => "AIStateSpiderDrop";

    public float GetUpDuration => ComponentData.GetUpDuration;
    public float FloorY => ComponentData.FloorY;
    public string[] SpawnerIds => ComponentData.SpawnerIds;
    public int[] NumberOfThrowPerPhase => ComponentData.NumberOfThrowPerPhase;

    public override void StartState() =>
        Position.SetPosition(Position.X, FloorY, Position.Z);

    public override ExtLevelEditor.ComponentSettings GetSettings() =>
        [Position.X.ToString(), FloorY.ToString(), Position.Z.ToString()];
}
