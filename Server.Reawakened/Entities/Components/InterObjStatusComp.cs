using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Entities.Components;

public class InterObjStatusComp : Component<InterObjStatus>
{
    public int DifficultyLevel => ComponentData.DifficultyLevel;
    public int GenericLevel => ComponentData.GenericLevel;
    public int Stars => ComponentData.Stars;
    public int MaxHealth => ComponentData.MaxHealth;
    public float LifebarOffsetX => ComponentData.LifeBarOffsetX;
    public float LifebarOffsetY => ComponentData.LifeBarOffsetY;
    public string EnemyLifeBar => ComponentData.EnemyLifeBar;

    public int StandardDamageResistPoints => ComponentData.StandardDamageResistPoints;
    public int FireDamageResistPoints => ComponentData.FireDamageResistPoints;
    public int IceDamageResistPoints => ComponentData.IceDamageResistPoints;
    public int PoisonDamageResistPoints => ComponentData.PoisonDamageResistPoints;
    public int LightningDamageResistPoints => ComponentData.LightningDamageResistPoints;

    public int AirDamageResistPoints => ComponentData.AirDamageResistPoints;
    public int EarthDamageResistPoints => ComponentData.EarthDamageResistPoints;
    public int StunStatusEffectResistSecs => ComponentData.StunStatusEffectResistSecs;
    public int SlowStatusEffectResistSecs => ComponentData.SlowStatusEffectResistSecs;
    public int FreezeStatusEffectResistSecs => ComponentData.FreezeStatusEffectResistSecs;

    public ILogger<InterObjStatusComp> Logger { get; set; }

    public QuestCatalog QuestCatalog { get; set; }
    public ObjectiveCatalogInt ObjectiveCatalog { get; set; }

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

        // Check if dead before running

        player.CheckObjective(QuestCatalog, ObjectiveCatalog, ObjectiveEnum.Score, Id, PrefabName, 1); // Unknown if needed?
        player.CheckObjective(QuestCatalog, ObjectiveCatalog, ObjectiveEnum.Scoremultiple, Id, PrefabName, 1);

        //player.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
        //player.SendUpdatedInventory(false);
    }
}
