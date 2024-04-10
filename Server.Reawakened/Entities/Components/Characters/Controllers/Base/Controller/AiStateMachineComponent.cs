using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
public abstract class AiStateMachineComponent<T> : Component<T> where T : AIStateMachine
{
    public GameObjectComponents PreviousState = [];

    public void GoToNextState(GameObjectComponents NewState)
    {
        if (Room == null || Room.IsObjectKilled(Id))
            return;

        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = PreviousState,
            GoToStates = NewState
        };

        PreviousState = NewState;

        Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, Room));
    }
}
