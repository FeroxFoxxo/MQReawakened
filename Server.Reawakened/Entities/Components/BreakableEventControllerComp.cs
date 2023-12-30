using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class BreakableEventControllerComp : Component<BreakableEventController>
{
    public override object[] GetInitData(Player player) => new object[] { 0 };
}
