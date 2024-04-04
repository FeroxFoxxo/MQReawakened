using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;
using Server.Reawakened.Entities.Components.Misc;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.Colliders;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Room = Server.Reawakened.Rooms.Room;

namespace Server.Reawakened.Entities.Components.GameObjects.Breakables;

public class BreakableEventControllerComp : Component<BreakableEventController>, IDestructible
{
    public ItemCatalog ItemCatalog { get; set; }
    public InternalLoot LootCatalog { get; set; }
    public TimerThread TimerThread { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<BreakableEventControllerComp> Logger { get; set; }

    public int MaxHealth { get; set; }
    public int NumberOfHits;
    public bool OverrideDeath;

    private int _health;
    private BaseSpawnerControllerComp _spawner;
    private IDamageable _damageable;

    public override void InitializeComponent()
    {
        NumberOfHits = 0;

        MaxHealth = 1;
        _health = MaxHealth;

        Room.Colliders.Add(Id, new BreakableCollider(Id, Position, Rectangle.Width, Rectangle.Height, ParentPlane, Room));
    }

    public void PostInit()
    {
        var spawner = Room.GetEntityFromId<BaseSpawnerControllerComp>(Id);
        var damagable = Room.GetEntityFromId<IDamageable>(Id);

        if (spawner != null)
        {
            _spawner = spawner;
            MaxHealth = _spawner.Health;
            OverrideDeath = true;
        }

        if (damagable != null)
        {
            _damageable = damagable;

            MaxHealth = damagable.MaxHealth;
            _damageable.CurrentHealth = MaxHealth;
        }

        _health = MaxHealth;
    }

    public void Damage(int damage, Elemental damageType, Player origin)
    {
        if (Room.IsObjectKilled(Id))
            return;

        var damagable = Room.GetEntityFromId<IDamageable>(Id);

        if (damagable != null)
            _health -= damagable.GetDamageAmount(damage, damageType);
        else
            _health -= damage;

        NumberOfHits++;

        Logger.LogInformation("Object name: {args1} Object Id: {args2}", PrefabName, Id);

        if (damagable is IBreakable breakable)
            if (breakable.NumberOfHitsToBreak > 0)
                if (NumberOfHits >= breakable.NumberOfHitsToBreak)
                    _health = 0;

        if (_damageable != null)
            _damageable.CurrentHealth = _health;

        origin.Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, _health, damage, 0, 0, origin.CharacterName, false, true));

        if (_health <= 0)
        {
            origin.GrantLoot(Id, LootCatalog, ItemCatalog, InternalAchievement, Logger);
            origin.SendUpdatedInventory();

            Room.KillEntity(origin, Id);
        }
    }

    public void Destroy(Player player, Room room, string id)
    {
        player?.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, ItemCatalog);

        room.Enemies.Remove(id);
        room.Colliders.Remove(id);
    }
}
