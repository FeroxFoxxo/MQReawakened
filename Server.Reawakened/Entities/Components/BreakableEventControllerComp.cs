using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Entities.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Room = Server.Reawakened.Rooms.Room;

namespace Server.Reawakened.Entities.Components;

public class BreakableEventControllerComp : Component<BreakableEventController>, IDestructible
{
    public ItemCatalog ItemCatalog { get; set; }
    public InternalLoot LootCatalog { get; set; }
    public ILogger<BreakableEventControllerComp> Logger { get; set; }

    private int _numberOfHits;
    private int _health;
    private BaseSpawnerControllerComp _spawner;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        _health = 1;
        _numberOfHits = 0;

        Room.Colliders.Add(Id, new BreakableCollider(Id, Position, Rectangle.Width, Rectangle.Height, ParentPlane, Room));
    }

    public void PostInit()
    {
        var spawner = Room.GetEntityFromId<BaseSpawnerControllerComp>(Id);

        if (spawner != null)
        {
            _spawner = spawner;
            _health = _spawner.Health;
        }
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
    }

    public void Damage(int damage, Elemental damageType, Player origin)
    {
        var damagable = Room.GetEntityFromId<IDamageable>(Id);
        
        _health -= damagable.GetDamageAmount(damage, damageType);
        _numberOfHits--;

        Logger.LogInformation("Object name: {args1} Object Id: {args2}", PrefabName, Id);

        var breakEvent = new AiHealth_SyncEvent(Id.ToString(), Room.Time, _health, damage, 0, 0, origin.CharacterName, false, true);
        origin.Room.SendSyncEvent(breakEvent);

        var broken = _health <= 0;

        if (damagable is IBreakable breakable)
            if (breakable.NumberOfHitsToBreak > 0)
                broken = _numberOfHits >= breakable.NumberOfHitsToBreak;

        if (broken)
        {
            origin.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
            origin.SendUpdatedInventory(false);
            Destroy(origin, Room, Id);
        }
    }

    public void Destroy(Player player, Room room, string id)
    {
        player.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, ItemCatalog);

        room.RemoveEntity(id);
        room.Enemies.Remove(id);
        room.Colliders.Remove(id);
    }
}
