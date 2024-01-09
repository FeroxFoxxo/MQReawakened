using Server.Reawakened.Entities.AIStates.SyncEvents;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.AIStates;

public class AIStateSpiderDropComp : Component<AIStateSpiderDrop>
{
    public float GetUpDuration => ComponentData.GetUpDuration;
    public float FloorY => ComponentData.FloorY;
    public string[] SpawnerIds => ComponentData.SpawnerIds;
    public int[] NumberOfThrowPerPhase => ComponentData.NumberOfThrowPerPhase;
}
