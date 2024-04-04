using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using static SyncEvent;

namespace Server.Reawakened.Entities.Components.GameObjects.Hazards;

public class DropControllerComp : Component<DropController>, IRecieverTriggered
{
    public float TimeOnGround => ComponentData.TimeOnGround;
    public float WarningTime => ComponentData.WarningTime;
    public float InitialVelocity => ComponentData.InitialVelocity;
    public float RespawnTime => ComponentData.RespawnTime;
    public bool StartActivated => ComponentData.StartActivated;

    public DropController.DropState DropState;
    public float Time;

    public override void InitializeComponent()
    {
        DropState = StartActivated ?
            DropController.DropState.Fallen :
            DropController.DropState.Stand;

        Time = 0f;
    }

    public override object[] GetInitData(Player player) => [(int)DropState, Time];

    public void RecievedTrigger(bool triggered)
    {
        var dropSyncEvent = new SyncEvent(Id, EventType.Drop, Room.Time);

        dropSyncEvent.EventDataList.Add((int)DropState);
        dropSyncEvent.EventDataList.Add(Position.X);
        dropSyncEvent.EventDataList.Add(Position.Y);

        Room.SendSyncEvent(dropSyncEvent);
    }
}
