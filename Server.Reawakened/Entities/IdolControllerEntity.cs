using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class IdolControllerEntity : SyncedEntity<IdolController>
{
    public override string[] GetInitData(NetState netState)
    {
        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();
        var levelId = Level.LevelInfo.LevelId;

        //if (character.Data.IdolCount.ContainsKey(levelId))
        //    if (character.Data.IdolCount[levelId].)
        //    return new [] { "0" };

        return Array.Empty<string>();
    }
}
