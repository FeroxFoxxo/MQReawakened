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
    
    // Provide Drop Position
    public override ExtLevelEditor.ComponentSettings GetSettings() => throw new NotImplementedException();
}
