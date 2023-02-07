using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class IdolControllerEntity : SyncedEntity<IdolController>
{
    public int Index => EntityData.Index;

    public override string[] GetInitData(NetState netState)
    {
        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();
        var levelId = Level.LevelInfo.LevelId;

        if (!character.CollectedIdols.ContainsKey(levelId))
            character.CollectedIdols.Add(levelId, new List<int>());
        
        return new [] { character.CollectedIdols[levelId].Contains(Index) ? "0" : "1" };
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();
        var levelId = Level.LevelInfo.LevelId;

        player.SentEntityTriggered(Id, Level);

        if (!character.CollectedIdols[levelId].Contains(Index))
            character.CollectedIdols[levelId].Add(Index);
    }
}
