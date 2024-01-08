using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Entities.Components;
public class QuestCollectibleControllerComp : Component<QuestCollectibleController>
{
    public bool Collected;
    public string CollectedFx => ComponentData.CollectedFx;
    public float GatherTime => ComponentData.GatherTime;
    public QuestCatalog QuestCatalog { get; set; }
    public ObjectiveCatalogInt ObjectiveCatalog { get; set; }

    public override object[] GetInitData(Player player) => [Collected ? 0 : 1];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        player.CheckObjective(QuestCatalog, ObjectiveCatalog, A2m.Server.ObjectiveEnum.Collect, Id, PrefabName, 1);

        var questCollectible = new Trigger_SyncEvent(syncEvent);
        Room.SendSyncEvent(questCollectible);

        base.RunSyncedEvent(syncEvent, player);
    }
}
