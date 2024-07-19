using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.GameObjects.Bouncers;

public class InvisibilityControllerComp : Component<InvisibilityController>
{
    public int DetectionLevelRequired => ComponentData.DetectionLevelRequired;
    public bool ApplyInvisibility => ComponentData.ApplyInvisibility;
}
