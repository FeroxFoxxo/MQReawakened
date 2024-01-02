using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using System.Drawing;
using UnityEngine;

namespace Server.Reawakened.Entities.Components;

public class BreakableEventControllerComp : Component<BreakableEventController>
{
    public ItemCatalog ItemCatalog { get; set; }
    public LootCatalogInt LootCatalog { get; set; }
    public ILogger<BreakableEventControllerComp> Logger { get; set; }

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        Room.Colliders.Add(Id, new BaseCollider(Id, Position, Rectangle.Width, Rectangle.Height, ParentPlane, Room));
    }
    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
    }

    public void Destroy(Player player)
    {
        Logger.LogInformation("Object name: {args1} Object Id: {args2}", PrefabName, Id);

        // Link to damage + health of object later
        var breakEvent = new AiHealth_SyncEvent(Id.ToString(), player.Room.Time, 0, 100, 0, 0, player.CharacterName, false, true);
        player.Room.SendSyncEvent(breakEvent);

        player.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
        player.SendUpdatedInventory(false);
        player.Room.Dispose(Id);
    }
}
