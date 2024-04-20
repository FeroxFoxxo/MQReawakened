using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
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

    public bool CanBreak = true;

    public override void InitializeComponent()
    {
        Damageable = Room.GetEntityFromId<BreakableObjStatusComp>(Id);

        var box = new Rect(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
        var position = new Vector3(Position.X, Position.Y, Position.Z);

        Room.AddCollider(new BreakableCollider(Id, position, box, ParentPlane, Room));
    }

    public void Damage(int damage, Elemental damageType, Player origin)
    {
        if (Room.IsObjectKilled(Id) || !CanBreak)
            return;

        Logger.LogInformation("Damaged object: '{PrefabName}' ({Id})", PrefabName, Id);

        RunDamage(damage, damageType);

        origin.Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Damageable.CurrentHealth, damage, 0, 0, origin.CharacterName, false, true));

        if (Damageable.CurrentHealth <= 0)
        {
            origin.GrantLoot(Id, LootCatalog, ItemCatalog, InternalAchievement, Logger);
            origin.SendUpdatedInventory();

            Room.KillEntity(origin, Id);
        }
    }

    public void RunDamage(int damage, Elemental damageType)
    {
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
                    Damageable.CurrentHealth = breakable.NumberOfHits / breakable.NumberOfHitsToBreak;

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

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }

    public void Destroy(Player player, Room room, string id)
    {
        player?.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, ItemCatalog);

        room.RemoveEnemy(id);
    }
}
