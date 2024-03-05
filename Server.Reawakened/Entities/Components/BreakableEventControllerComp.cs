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
        var damagable = Room.GetEntityFromId<IDamageable>(Id);

        if (spawner != null)
        {
            _spawner = spawner;
            _health = _spawner.Health;
        }
        else if (damagable != null)
        {
            _health = damagable.MaxHealth;
        }
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
    }

    public void Damage(int damage, Elemental damageType, Player origin)
    {
        var damagable = Room.GetEntityFromId<IDamageable>(Id);

        if (damagable != null)
            _health -= damagable.GetDamageAmount(damage, damageType);
        else
            _health -= damage;

        _numberOfHits++;

        Logger.LogInformation("Object name: {args1} Object Id: {args2}", PrefabName, Id);

        var broken = _health <= 0;

        if (damagable is IBreakable breakable)
            if (breakable.NumberOfHitsToBreak > 0)
            {
                if (_numberOfHits < breakable.NumberOfHitsToBreak)
                    origin.Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, _health, damage, 0, 0, origin.CharacterName, false, true));
                else
                    origin.Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, 0, damage, 0, 0, origin.CharacterName, false, true));
                broken = _numberOfHits >= breakable.NumberOfHitsToBreak;
            }
            else
                origin.Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, _health, damage, 0, 0, origin.CharacterName, false, true));

        if (broken)
        {
            origin.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
            origin.SendUpdatedInventory(false);

            foreach (var destructable in Room.GetEntitiesFromId<IDestructible>(Id))
                destructable.Destroy(origin, Room, Id);

            Room.RemoveEntity(Id);
        }
    }

    public void Destroy(Player player, Room room, string id)
    {
        player.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, ItemCatalog);

        room.Enemies.Remove(id);
        room.Colliders.Remove(id);
    }
}
