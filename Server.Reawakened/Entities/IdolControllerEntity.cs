using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class IdolControllerEntity : SyncedEntity<IdolController>
{
    public int Index => EntityData.Index;

    public override object[] GetInitData(NetState netState)
    {
        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();
        var levelId = Level.LevelInfo.LevelId;

        if (!character.CollectedIdols.ContainsKey(levelId))
            character.CollectedIdols.Add(levelId, new List<int>());

        return character.CollectedIdols[levelId].Contains(Index) ? new object[] { 0 } : Array.Empty<object>();
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();
        var levelId = Level.LevelInfo.LevelId;

        if (character.CollectedIdols[levelId].Contains(Index))
            return;

        character.CollectedIdols[levelId].Add(Index);
        player.SentEntityTriggered(Id, Level);
    }
}
