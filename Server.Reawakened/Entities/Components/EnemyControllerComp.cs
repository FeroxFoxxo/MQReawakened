using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using SmartFoxClientAPI.Data;
using UnityEngine;
using Room = Server.Reawakened.Rooms.Room;

namespace Server.Reawakened.Entities.Components;
public class EnemyControllerComp : Component<EnemyController>, IDestructible
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
    public bool CanAutoScale => ComponentData.CanAutoScale;
    public bool CanAutoScaleResistance => ComponentData.CanAutoScaleResistance;
    public bool CanAutoScaleDamage => ComponentData.CanAutoScaleDamage;

    public ILogger<EnemyControllerComp> Logger { get; set; }
    public InternalDefaultEnemies EnemyInfoXml { get; set; }

    public int Level;
    public override void InitializeComponent()
    {
        Level = Room.LevelInfo.Difficulty + EnemyLevelOffset;
    }
    public override void RunSyncedEvent(SyncEvent syncEvent, Player player) => base.RunSyncedEvent(syncEvent, player);

    public void Damage(int damage, Player origin)
    {
        var breakEvent = new AiHealth_SyncEvent(Id.ToString(), Room.Time, 0, damage, 0, 0, origin.CharacterName, false, true);
        origin.Room.SendSyncEvent(breakEvent);

        origin.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1);
        origin.CheckObjective(ObjectiveEnum.Scoremultiple, Id, PrefabName, 1);

        if (Room.Entities.TryGetValue(Id, out var comps))
            foreach (var comp in comps)
                if (comp is IDestructible dest)
                    dest.Destroy(Room, Id);

        Room.Entities.Remove(Id);
    }

    public void Destroy(Room room, string id)
    {
        room.Enemies.Remove(id);
        room.Colliders.Remove(id);
    }
}
