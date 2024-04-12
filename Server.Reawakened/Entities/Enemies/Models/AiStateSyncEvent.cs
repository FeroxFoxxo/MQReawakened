using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Enemies.Models;

public class AiStateSyncEvent()
{
    public GameObjectComponents InStates = [];
    public GameObjectComponents GoToStates = [];

    public SyncEvent GetSyncEvent(string id, Room room)
    {
        var aiEvent = new SyncEvent(id.ToString(), SyncEvent.EventType.AIState, room.Time);

        aiEvent.EventDataList.Add(GetAllStates());

        return aiEvent;
    }

    private string GetAllStates()
    {
        var sb = new SeparatedStringBuilder('|');

        sb.Append(GetStates(InStates));
        sb.Append(GetStates(GoToStates));

        return sb.ToString();
    }

    private static string GetStates(GameObjectComponents states)
    {
        var sb = new SeparatedStringBuilder('#');

        foreach (var state in states)
            sb.Append(GetState(state));

        return sb.ToString();
    }

    private static string GetState(KeyValuePair<string, ComponentSettings> state)
    {
        var sb = new SeparatedStringBuilder('~');

        sb.Append(state.Key);

        foreach (var component in state.Value)
            sb.Append(component);

        return sb.ToString();
    }
}
