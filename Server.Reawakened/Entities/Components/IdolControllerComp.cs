using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Enums;

namespace Server.Reawakened.Entities.Components;

public class IdolControllerComp : Component<IdolController>
{
    public int Index => ComponentData.Index;

    public override object[] GetInitData(Player player)
    {
        var character = player.Character;
        var levelId = Room.LevelInfo.LevelId;

        if (!character.CollectedIdols.ContainsKey(levelId))
            character.CollectedIdols.Add(levelId, []);

        return character.CollectedIdols[levelId].Contains(Index) ? [0] : [];
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        var character = player.Character;
        var levelId = Room.LevelInfo.LevelId;

        if (character.CollectedIdols[levelId].Contains(Index))
            return;

        character.CollectedIdols[levelId].Add(Index);

        player.CheckAchievement(AchConditionType.CollectIdol, Room.LevelInfo.Name);

        var collectedEvent =
            new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true);

        player.SendSyncEventToPlayer(collectedEvent);

        player.CheckObjective(ObjectiveEnum.IdolCollect, Id, PrefabName, 1);
    }
}
