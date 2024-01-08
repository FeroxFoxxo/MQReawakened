using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Components;
public class EnemyControllerComp : Component<EnemyController>
{
    public int OnKillRepPoints => ComponentData.OnKillRepPoints;
    public bool TopBounceImmune => ComponentData.TopBounceImmune;
    public string OnKillMessageReceiver => ComponentData.OnKillMessageReceiver;
    public int EnemyLevelOffset => ComponentData.EnemyLevelOffset;
    public string OnDeathTargetID => ComponentData.OnDeathTargetID;
    public string EnemyDisplayName => ComponentData.EnemyDisplayName;
    public int EnemyDisplayNameSize => ComponentData.EnemyDisplayNameSize;
    public Vector3 EnemyDisplayNamePosition => ComponentData.EnemyDisplayNamePosition;
    public Color EnemyDisplayNameColor => ComponentData.EnemyDisplayNameColor;
    public EnemyScalingType EnemyScalingType => ComponentData.EnemyScalingType;
    public ILogger<EnemyControllerComp> Logger { get; set; }

    public override void InitializeComponent() => base.InitializeComponent();
    public override void RunSyncedEvent(SyncEvent syncEvent, Player player) => base.RunSyncedEvent(syncEvent, player);

    public void SendDamageEvent(Player player)
    {
        Logger.LogInformation("Enemy name: {args1} Enemy Id: {args2}", PrefabName, Id);

        // Link to damage + health of object later
        var damageEvent = new AiHealth_SyncEvent(Id.ToString(), player.Room.Time, 0, 5, 0, 0, player.CharacterName, false, true);
        player.Room.SendSyncEvent(damageEvent);

        //player.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
        //player.SendUpdatedInventory(false);
    }
}
