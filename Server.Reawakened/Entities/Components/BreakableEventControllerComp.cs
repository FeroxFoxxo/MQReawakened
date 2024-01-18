using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;

namespace Server.Reawakened.Entities.Components;

public class BreakableEventControllerComp : Component<BreakableEventController>, IDestructible
{
    public ItemCatalog ItemCatalog { get; set; }
    public InternalLoot LootCatalog { get; set; }
    public ILogger<BreakableEventControllerComp> Logger { get; set; }

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        Room.Colliders.Add(Id, new BreakableCollider(Id, Position, Rectangle.Width, Rectangle.Height, ParentPlane, Room));
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
    }

    public void Damage(int damage, Player origin)
    {
        Logger.LogInformation("Object name: {args1} Object Id: {args2}", PrefabName, Id);
        var breakEvent = new AiHealth_SyncEvent(Id.ToString(), Room.Time, 0, 100, 0, 0, origin.CharacterName, false, true);
        origin.Room.SendSyncEvent(breakEvent);
        Destroy(Room, Id);

        origin.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
        origin.SendUpdatedInventory(false);
    }
    public void Destroy(Room room, int id)
    {
        room.Entities.Remove(id);
        room.Enemies.Remove(id);
        room.Colliders.Remove(id);
    }
}
