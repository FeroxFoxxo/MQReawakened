using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.GameObjects.Hazards;
public class DroppingsControllerComp : Component<DroppingsController>
{
    public float DropRate => ComponentData.DropRate;

    private Vector3Model _startPosition;
    private float _activationStartTime = 0;

    public override void InitializeComponent() => 
        _startPosition = new Vector3Model(Position.X, Position.Y, Position.Z);

    public override void Update()
    {
        if (Room == null || Room.IsObjectKilled(Id))
            return;

        base.Update();

        if (DropRate > 0)
        {
            var time = Room.Time;
            if (time - _activationStartTime > DropRate)
            {
                _activationStartTime += DropRate;
                SendDrop();
            }
        }
    }

    public void SendDrop()
    {
        Position.SetPosition(_startPosition);

        var speed = new Vector2
        {
            x = 0,
            y = -6
        };

        var damage = 0;
        var effect = ItemEffectType.Freezing;

        Room.AddRangedProjectile(Id, Position, speed, 3, damage, effect, false, PrefabName);
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
