using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class InterObjStatusComp : Component<InterObjStatus>
{
    public int DifficultyLevel => ComponentData.DifficultyLevel;
    public int GenericLevel => ComponentData.GenericLevel;
    public int Stars => ComponentData.Stars;
    public int MaxHealth => ComponentData.MaxHealth;
    public float LifebarOffsetX => ComponentData.LifeBarOffsetX;
    public float LifebarOffsetY => ComponentData.LifeBarOffsetY;
    public ILogger<InterObjStatusComp> Logger { get; set; }
    public override void InitializeComponent()
    {
        //Fix spawn position for duplicate position args when spawners are added
        base.InitializeComponent();
        Room.Colliders.Add(Id, new BaseCollider(Id, Position, Rectangle.Width, Rectangle.Height, ParentPlane, Room));
        Disposed = false;
    }
    public void SendDamageEvent(Player player)
    {

        Logger.LogInformation("Enemy name: {args1} Enemy Id: {args2}", PrefabName, Id);

        // Link to damage + health of object later
        var damageEvent = new AiHealth_SyncEvent(Id.ToString(), player.Room.Time, ComponentData.Health, 5, 0, 0, player.CharacterName, false, true);
        player.Room.SendSyncEvent(damageEvent);
        player.Room.Dispose(Id);

        //player.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
        //player.SendUpdatedInventory(false);
    }
}
