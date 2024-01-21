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
using Server.Reawakened.Entities.Stats;
using SmartFoxClientAPI.Data;
using Room = Server.Reawakened.Rooms.Room;
using Server.Base.Core.Abstractions;
using Server.Reawakened.Entities.Entity.Enemies;

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
        var entityList = Room.Entities.Values.SelectMany(s => s);
        foreach (var entity in entityList)
        {
            if (entity.Id == Id && entity is BreakableObjStatusComp status)
                _status = status;
            else if (entity.Id == Id && entity is BaseSpawnerControllerComp spawner)
            {
                _spawner = spawner;
                _health = _spawner.Health;
                _isSpawner = true;
            }
        }
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
    }

    public void Damage(int damage, Player origin)
    {
        _health -= damage;
        Console.WriteLine(_health);
        Logger.LogInformation("Object name: {args1} Object Id: {args2}", PrefabName, Id);

        var breakEvent = new AiHealth_SyncEvent(Id.ToString(), Room.Time, _health, damage, 0, 0, origin.CharacterName, false, true);
        origin.Room.SendSyncEvent(breakEvent);

        if (_health <= 0)
        {
            origin.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
            origin.SendUpdatedInventory(false);
            Destroy(Room, Id);
        }
    }
    public void Destroy(Room room, int id)
    {
        room.Entities.Remove(id);
        room.Enemies.Remove(id);
        room.Colliders.Remove(id);
    }
}
