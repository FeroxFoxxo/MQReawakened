using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities;

public class IdolControllerEntity : SyncedEntity<IdolController>
{
    public int Index => EntityData.Index;

    public override object[] GetInitData(Player player)
    {
        var character = player.Character;
        var levelId = Room.LevelInfo.LevelId;

        if (!character.CollectedIdols.ContainsKey(levelId))
            character.CollectedIdols.Add(levelId, new List<int>());

        return character.CollectedIdols[levelId].Contains(Index) ? new object[] { 0 } : Array.Empty<object>();
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        var character = player.Character;
        var levelId = Room.LevelInfo.LevelId;

        if (character.CollectedIdols[levelId].Contains(Index))
            return;

        character.CollectedIdols[levelId].Add(Index);

        var collectedEvent =
            new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true);

        player.SendSyncEventToPlayer(collectedEvent);
    }
}
