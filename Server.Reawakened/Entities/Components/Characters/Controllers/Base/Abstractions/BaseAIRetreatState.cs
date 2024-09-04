using Server.Base.Core.Abstractions;
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
        {
            TimerThread.RunDelayed(OpenDoor, this, TimeSpan.FromSeconds(DelayUntilOpen));
        }
    }

    public static void OpenDoor(ITimerData data)
    {
        if (data is not BaseAIRetreatState<T> retreat)
            return;

        foreach (var trigReceiver in retreat.Room.GetEntitiesFromId<TriggerReceiverComp>(retreat.DoorId.ToString()))
            trigReceiver.Trigger(true, retreat.Id);
    }
}
