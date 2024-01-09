using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.AIStates;

public class AIStateSpiderDropComp : Component<AIStateSpiderDrop>
{
    public float GetUpDuration => ComponentData.GetUpDuration;
    public float FloorY => ComponentData.FloorY;
    public string[] SpawnerIds => ComponentData.SpawnerIds;
    public int[] NumberOfThrowPerPhase => ComponentData.NumberOfThrowPerPhase;
}
