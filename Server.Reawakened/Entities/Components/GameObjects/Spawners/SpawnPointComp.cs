using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.GameObjects.Spawners;

public class SpawnPointComp : Component<SpawnPoint>
{
    public int Index => ComponentData.Index;
}
