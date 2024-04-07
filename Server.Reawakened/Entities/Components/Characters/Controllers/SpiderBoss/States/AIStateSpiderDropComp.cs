using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;

public class AIStateSpiderDropComp : Component<AIStateSpiderDrop>
{
    public float GetUpDuration => ComponentData.GetUpDuration;
    public float FloorY => ComponentData.FloorY;
    public string[] SpawnerIds => ComponentData.SpawnerIds;
    public int[] NumberOfThrowPerPhase => ComponentData.NumberOfThrowPerPhase;
}
