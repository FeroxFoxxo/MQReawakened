using A2m.Server;
using Microsoft.Extensions.Logging;
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

    private int _health;
    private BreakableObjStatusComp _status;
    private BaseSpawnerControllerComp _spawner;
    private bool _isSpawner;

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        _health = 1;
        _isSpawner = false;
        Room.Colliders.Add(Id, new BreakableCollider(Id, Position, Rectangle.Width, Rectangle.Height, ParentPlane, Room));
    }

    public void PostInit()
    {
        var status = Room.GetEntityFromId<BreakableObjStatusComp>(Id);

        if (status != null)
            _status = status;

        var spawner = Room.GetEntityFromId<BaseSpawnerControllerComp>(Id);

        if (spawner != null)
        {
            _spawner = spawner;
            _health = _spawner.Health;
            _isSpawner = true;
        }
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
    }

    public void Damage(int damage, Elemental damageType, Player origin)
    {
        damage = GetDamageType(damage, damageType);

        _health -= damage;
        Logger.LogInformation("Object name: {args1} Object Id: {args2}", PrefabName, Id);

        var breakEvent = new AiHealth_SyncEvent(Id.ToString(), Room.Time, _health, damage, 0, 0, origin.CharacterName, false, true);
        origin.Room.SendSyncEvent(breakEvent);

        if (_health <= 0)
        {
            origin.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
            origin.SendUpdatedInventory(false);
            Destroy(origin, Room, Id);
        }
    }

    public int GetDamageType(int damage, Elemental damageType)
    {
        var status = Room.GetEntityFromId<BreakableObjStatusComp>(Id);

        if (status != null)
        {
            if (damageType == Elemental.Air) damage -= status.ComponentData.AirDamageResistPoints;
            if (damageType == Elemental.Fire) damage -= status.ComponentData.FireDamageResistPoints;
            if (damageType == Elemental.Ice) damage -= status.ComponentData.IceDamageResistPoints;
            if (damageType == Elemental.Earth) damage -= status.ComponentData.EarthDamageResistPoints;
            if (damageType == Elemental.Poison) damage -= status.ComponentData.PoisonDamageResistPoints;
            if (damageType is Elemental.Standard or Elemental.Invalid or Elemental.Unknown)
                damage -= status.ComponentData.AirDamageResistPoints;
        }

        else
        {
            var intrenalStatus = Room.GetEntityFromId<InterObjStatusComp>(Id);

            if (damageType == Elemental.Air) damage -= intrenalStatus.AirDamageResistPoints;
            if (damageType == Elemental.Fire) damage -= intrenalStatus.FireDamageResistPoints;
            if (damageType == Elemental.Ice) damage -= intrenalStatus.IceDamageResistPoints;
            if (damageType == Elemental.Earth) damage -= intrenalStatus.EarthDamageResistPoints;
            if (damageType == Elemental.Poison) damage -= intrenalStatus.PoisonDamageResistPoints;
            if (damageType is Elemental.Standard or Elemental.Invalid or Elemental.Unknown)
                damage -= intrenalStatus.AirDamageResistPoints;
        }

        if (damage < 0) damage = 0;

        return damage;
    }

    public void Destroy(Player player, Room room, string id)
    {
        player.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, ItemCatalog);

        room.RemoveEntity(id);
        room.Enemies.Remove(id);
        room.Colliders.Remove(id);
    }
}
