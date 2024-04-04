using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Players;
using A2m.Server;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.Configs;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.GameObjects.Hazards;
public class DroppingsControllerComp : Component<DroppingsController>
{
    public float DropRate => ComponentData.DropRate;

    public static Vector3Model StartPosition => new();
    public TimerThread TimerThread { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }

    public override void InitializeComponent()
    {
        ChangePosition(StartPosition, Position);
        WaitDrop();
    }

    public void WaitDrop() =>
        TimerThread.DelayCall(SendDrop, null, TimeSpan.FromSeconds(DropRate), TimeSpan.FromSeconds(1), 1);

    public void SendDrop(object _)
    {
        if (Room.KilledObjects.Contains(Id))
            return;

        ChangePosition(Position, StartPosition);

        var position = new Vector3
        {
            x = Position.X,
            y = Position.Y,
            z = Position.Z
        };

        var speed = new Vector2
        {
            x = -5,
            y = 3
        };

        var damage = 0;
        var effect = ItemEffectType.Freezing;

        Room.AddRangedProjectile(Id, position, speed, 3, damage, effect, false);

        WaitDrop();
    }

    public void FreezePlayer(Player player)
    {
        SendFreezeEvent(player, ItemEffectType.IceDamage, 1, 5, true, false);
        SendFreezeEvent(player, ItemEffectType.Freezing, 1, 5, true, false);
        SendFreezeEvent(player, ItemEffectType.FreezingStatusEffect, 1, 5, true, false);
    }

    private void SendFreezeEvent(Player player, ItemEffectType effect, int amount,
            int duration, bool start, bool isPremium) =>
        Room.SendSyncEvent(
            new StatusEffect_SyncEvent(
                player.GameObjectId, Room.Time,
                (int)effect, amount, duration,
                start, Id, isPremium
            )
        );

    public static void ChangePosition(Vector3Model to, Vector3Model from)
    {
        to.X = from.X;
        to.Y = from.Y;
        to.Z = from.Z;
    }
}
