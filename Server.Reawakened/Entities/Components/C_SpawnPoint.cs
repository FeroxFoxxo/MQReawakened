using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class C_SpawnPoint : Component<SpawnPoint>
{
    public int Index => ComponentData.Index;
}
