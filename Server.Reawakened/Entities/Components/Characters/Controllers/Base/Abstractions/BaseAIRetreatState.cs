using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public abstract class BaseAIRetreatState<T> : BaseAIState<T>
{
    public abstract int DoorId { get; }
    public abstract float DelayUntilOpen { get; }

    public TimerThread TimerThread { get; set; }

    public override void StartState()
    {
        if (DoorId > 0)
            TimerThread.DelayCall(OpenDoor, null, TimeSpan.FromSeconds(DelayUntilOpen), TimeSpan.Zero, 1);
    }

    public void OpenDoor(object _)
    {
        if (Room == null)
            return;

        foreach (var trigReceiver in Room.GetEntitiesFromId<TriggerReceiverComp>(DoorId.ToString()))
            trigReceiver.Trigger(true);
    }
}
