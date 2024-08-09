using Server.Base.Core.Abstractions;
using Server.Reawakened.Players;

namespace Server.Reawakened.Rooms.Models.Timers;
public class PlayerTimer : ITimerData
{
    public Player Player { get; set; }
    public virtual bool IsValid() => Player != null && Player.Character != null && Player.TempData != null;
}
