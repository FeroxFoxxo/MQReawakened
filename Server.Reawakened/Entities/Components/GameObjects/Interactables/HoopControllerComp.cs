using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.Misc;

public class HoopControllerComp : Component<HoopController>
{
    public bool IsMasterController => ComponentData.IsMasterController;
    public int HoopGroupId => ComponentData.HoopGroupId;
    public string HoopGroupStringId => ComponentData.HoopGroupStringId;
    public float TimeBeforeReset => ComponentData.TimeBeforeReset;
}
