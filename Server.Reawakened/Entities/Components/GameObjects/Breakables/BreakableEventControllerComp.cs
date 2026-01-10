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

    public bool CanBreak = true;

    private BaseSpawnerControllerComp _spawner;
    private BreakableObjStatusComp _status;
    private SpiderBreakableComp _spider;

    public override void InitializeComponent()
    {
        _spawner = Room.GetEntityFromId<BaseSpawnerControllerComp>(Id);
        _status = Room.GetEntityFromId<BreakableObjStatusComp>(Id);
        _spider = Room.GetEntityFromId<SpiderBreakableComp>(Id);

        Damageable = _status;
        Damageable ??= _spider;

        if (_spawner is not null && _spawner.HasLinkedArena)
            CanBreak = false;

        // EnemyTarget does not exist in 2012 level xmls
        var enemyTarget = ItemCatalog.Config.GameVersion >= Core.Enums.GameVersion.vEarly2013 && (_status?.EnemyTarget ?? false);

        _ = new BreakableCollider(this, enemyTarget);
    }

    // In 2012 BreakableEventController has an OnInitEvent method
    public override object[] GetInitData(Player player) => Damageable != null
            ? [Damageable.CurrentHealth + "|" + Damageable.MaxHealth]
            : _spawner != null ? [_spawner.CurrentHealth + "|" + _spawner.MaxHealth] : ["5|5"];

    public void Damage(int damage, Elemental damageType, Player origin)
    {
        if (Room.IsObjectKilled(Id) || !CanBreak || Damageable is null)
        {
            Logger.LogWarning("Attempted to damage an unbreakable or already destroyed object: '{PrefabName}' ({Id}), Can Break: {CanBreak}, Damageable: {Damageable}", PrefabName, Id, CanBreak, Damageable != null);
            return;
        }

        Logger.LogInformation("Damaged object: '{PrefabName}' ({Id})", PrefabName, Id);

        damage = RunDamage(damage, damageType);

        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Damageable.CurrentHealth, damage, 0, 0, origin == null ? string.Empty : origin.CharacterName, false, true));

        if (Damageable.CurrentHealth <= 0)
        {
            TriggerDeathTargets();

            if (origin != null)
            {
                var isGroup = origin.TempData.Group != null;

                if (isGroup)
                {
                    foreach (var groupie in origin.TempData.Group.GetMembers().Where(groupie => groupie.Room == origin.Room))
                    {
                        groupie.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, ItemCatalog);
                        groupie.CheckObjective(ObjectiveEnum.Scoremultiple, Id, PrefabName, 1, ItemCatalog);
                        groupie.SendUpdatedInventory(false);
                    }
                }
                else
                {
                    origin.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, ItemCatalog);
                    origin.CheckObjective(ObjectiveEnum.Scoremultiple, Id, PrefabName, 1, ItemCatalog);
                    origin.SendUpdatedInventory(false);
                }

                origin.GrantLoot(Id, LootCatalog, ItemCatalog, InternalAchievement, Logger);
            }

            if (_spawner is null)
            {
                Room.KillEntity(Id);
                Destroy(Room, Id);
            }
            else if (!_spawner.HasLinkedArena)
            {
                Room.KillEntity(Id);
                _spawner.Destroy();
                _spawner.PingDeathTargets();
            }
            else
            {
                _spawner.SetActive(false);
                _spawner.RemoveFromArena();
                _spawner.PingDeathTargets();
                Room.ToggleCollider(Id, false);
            }
        }
    }

    public void Damage(int damage, string enemyId)
    {
        if (Room.IsObjectKilled(Id) || !CanBreak || Damageable is null)
            return;

        Logger.LogInformation("Damaged object (from enemy): '{PrefabName}' ({Id})", PrefabName, Id);

        damage = RunDamage(damage, Elemental.Standard);

        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Damageable.CurrentHealth, damage, 0, 0, enemyId, false, true));

        if (Damageable.CurrentHealth <= 0)
        {
            Room.KillEntity(Id);
            Destroy(Room, Id);
        }
    }

    public int RunDamage(int damage, Elemental damageType)
    {
        if (Damageable is null)
            return 0;

        var dmgAmount = Damageable.GetDamageAmount(damage, damageType);

        if (Damageable is IBreakable breakable)
        {
            // This is here so that if the damage is totally resisted, the obj won't break.
            // See Boom Barrels (PF_OUT_BARRELTNT)
            if (dmgAmount <= 1)
                return 0;

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

                return dmgAmount;
            }
        }

        Damageable.CurrentHealth -= dmgAmount;

        if (Damageable.CurrentHealth < 0)
            Damageable.CurrentHealth = 0;

        return dmgAmount;
    }

    public void Respawn()
    {
        Logger.LogInformation("Revived object: '{PrefabName}' ({Id})", PrefabName, Id);
        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Damageable.MaxHealth, 0, 0, 0, string.Empty, false, false));
        Damageable.CurrentHealth = Damageable.MaxHealth;
        _status.NumberOfHits = 0;
    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }

    public void Destroy(Room room, string id) => room.RemoveEnemy(id);

    public void TriggerDeathTargets()
    {
        // Do not add a line for spawner, it is already handled in BaseSpawnerControllerComp

        if (_status != null && _status.OnKillMessageReceiver != string.Empty)
            foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(_status.OnKillMessageReceiver))
                trigger.TriggerStateChange(TriggerType.Activate, true, Id);

        else if (_spider != null && _spider.OnKillMessageReceiver != string.Empty)
            foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(_spider.OnKillMessageReceiver))
                trigger.TriggerStateChange(TriggerType.Activate, true, Id);
    }
}
