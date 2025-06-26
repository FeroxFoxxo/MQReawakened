using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;

namespace Server.Reawakened.Entities.Components.GameObjects.Items;

public class IdolControllerComp : Component<IdolController>
{
    public int Index => ComponentData.Index;

    public ILogger<HarvestControllerComp> Logger { get; set; }
    public InternalAchievement Achievement { get; set; }
    public QuestCatalog QuestCatalog { get; set; }

    public override object[] GetInitData(Player player)
    {
        var levelId = Room.LevelInfo.LevelId;

        if (!player.Character.CollectedIdols.ContainsKey(levelId))
            player.Character.CollectedIdols.Add(levelId, []);

        return player.Character.CollectedIdols[levelId].Contains(Index) ? [0] : [];
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        var levelId = Room.LevelInfo.LevelId;

        if (player.Character.CollectedIdols[levelId].Contains(Index))
            return;

        player.Character.CollectedIdols[levelId].Add(Index);

        var collectedEvent =
            new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true);

        player.SendSyncEventToPlayer(collectedEvent);

        player.SetObjective(ObjectiveEnum.IdolCollect, Id, PrefabName, player.Character.CollectedIdols[levelId].Count, QuestCatalog);
        player.CheckAchievement(AchConditionType.CollectIdol, [Room.LevelInfo.Name], Achievement, Logger);
    }
}
