using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Spawners;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Entities.Components.GameObjects.WowMoment;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using UnityEngine;
using Room = Server.Reawakened.Rooms.Room;

namespace Server.Reawakened.Entities.Components.GameObjects.Breakables;

public class BreakableEventControllerComp : Component<BreakableEventController>, IDestructible
{
    public ItemCatalog ItemCatalog { get; set; }
    public InternalLoot LootCatalog { get; set; }
    public TimerThread TimerThread { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<BreakableEventControllerComp> Logger { get; set; }

    public IDamageable Damageable;

    public BreakableObjStatusComp ObjStatus;

    public bool CanBreak = true;

    private BaseSpawnerControllerComp _spawner;

    public override void InitializeComponent()
    {
        ObjStatus = Room.GetEntityFromId<BreakableObjStatusComp>(Id);

        Damageable = ObjStatus;
        Damageable ??= Room.GetEntityFromId<SpiderBreakableComp>(Id);

        _spawner = Room.GetEntityFromId<BaseSpawnerControllerComp>(Id);
        if (_spawner is not null && _spawner.HasLinkedArena)
            CanBreak = false;

        var box = new Rect(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
        var position = new Vector3(Position.X, Position.Y, Position.Z);

        if (ObjStatus == null)
            return;

        Room.AddCollider(new BreakableCollider(Id, position, box, ParentPlane, Room, ObjStatus.EnemyTarget));
    }

    public void Damage(int damage, Elemental damageType, Player origin)
    {
        if (Room.IsObjectKilled(Id) || !CanBreak || Damageable is null)
            return;

        Logger.LogInformation("Damaged object: '{PrefabName}' ({Id})", PrefabName, Id);

        RunDamage(damage, damageType);

        origin.Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Damageable.CurrentHealth, damage, 0, 0, origin.CharacterName, false, true));

        if (Damageable.CurrentHealth <= 0)
        {
            if (_spawner is not null && _spawner.OnDeathTargetID is not null and not "0")
                foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(_spawner.OnDeathTargetID))
                    trigger.TriggerStateChange(TriggerType.Activate, true, Id);

            origin.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, ItemCatalog);
            origin.GrantLoot(Id, LootCatalog, ItemCatalog, InternalAchievement, Logger);
            origin.SendUpdatedInventory();

            if (_spawner is null || !_spawner.HasLinkedArena)
            {
                Room.KillEntity(Id);
                Destroy(Room, Id);
            }
            else
            {
                _spawner.SetActive(false);
                _spawner.RemoveFromArena();
                Room.ToggleCollider(Id, false);
            }

        }
    }

    public void Damage(int damage, string enemyId)
    {
        if (Room.IsObjectKilled(Id) || !CanBreak || Damageable is null)
            return;

        Logger.LogInformation("Damaged object (from enemy): '{PrefabName}' ({Id})", PrefabName, Id);

        RunDamage(damage, Elemental.Standard);

        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Damageable.CurrentHealth, damage, 0, 0, enemyId, false, true));

        if (Damageable.CurrentHealth <= 0)
        {
            Room.KillEntity(Id);
            Destroy(Room, Id);
        }
    }

    public void RunDamage(int damage, Elemental damageType)
    {
        if (Damageable is null)
            return;

        if (Damageable is IBreakable breakable)
        {
            breakable.NumberOfHits++;

            if (breakable.NumberOfHitsToBreak > 0)
            {
                if (breakable.NumberOfHits >= breakable.NumberOfHitsToBreak)
                {
                    Damageable.CurrentHealth = 0;
                }
                else
                {
                    Damageable.CurrentHealth = Convert.ToInt32(
                        Math.Floor(
                            Damageable.MaxHealth * ((double)(breakable.NumberOfHitsToBreak - breakable.NumberOfHits) / breakable.NumberOfHitsToBreak)
                        )
                    );

                    if (Damageable.CurrentHealth <= 0)
                        Damageable.CurrentHealth = 1;
                }

                return;
            }
        }

        Damageable.CurrentHealth -= Damageable.GetDamageAmount(damage, damageType);

        if (Damageable.CurrentHealth < 0)
            Damageable.CurrentHealth = 0;
    }

    public void Respawn()
    {
        Logger.LogInformation("Revived object: '{PrefabName}' ({Id})", PrefabName, Id);
        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Damageable.MaxHealth, 0, 0, 0, string.Empty, false, false));
        Damageable.CurrentHealth = Damageable.MaxHealth;

    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }

    public void Destroy(Room room, string id) => room.RemoveEnemy(id);
}
