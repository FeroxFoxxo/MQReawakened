using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.GameObjects.Hazards;
public class DroppingsControllerComp : Component<DroppingsController>
{
    public float DropRate => ComponentData.DropRate;

    public TimerThread TimerThread { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }

    private Vector3Model _startPosition;

    public override void InitializeComponent()
    {
        _startPosition = new Vector3Model(Position.X, Position.Y, Position.Z);
        WaitDrop();
    }

    public void WaitDrop() =>
        TimerThread.RunDelayed(SendDrop, this, TimeSpan.FromSeconds(DropRate));

    public static void SendDrop(ITimerData data)
    {
        if (data is not DroppingsControllerComp dropping)
            return;

        if (dropping.Room.IsObjectKilled(dropping.Id))
            return;

        dropping.Position.SetPosition(dropping._startPosition);

        var speed = new Vector2
        {
            x = 0,
            y = -6
        };

        var damage = 0;
        var effect = ItemEffectType.Freezing;

        dropping.Room.AddRangedProjectile(dropping.Id, dropping.Position, speed, 3, damage, effect, false);

        dropping.WaitDrop();
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
}
