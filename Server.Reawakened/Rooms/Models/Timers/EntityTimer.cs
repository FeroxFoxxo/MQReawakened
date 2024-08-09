using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Rooms.Models.Timers;
public abstract class EntityTimer : ITimerData
{
    public abstract Room Room { get; }

    public bool IsValid() => Room != null && Room.IsOpen;
}
