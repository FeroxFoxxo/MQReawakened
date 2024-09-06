using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Models.Timers;
using UnityEngine;
using Random = System.Random;

namespace Server.Reawakened.Players.Extensions;
public static class PlayerItemExtensions
{
    public static void HandleDrop(this Player player, ItemRConfig config, TimerThread timerThread,
        Microsoft.Extensions.Logging.ILogger logger, ItemDescription usedItem, Vector3 position, int direction)
    {
        var isLeft = direction > 0;
        var dropDirection = isLeft ? 1 : -1;
        var platform = new GameObjectModel();
        var planeName = player.GetPlayersPlaneString();

        var dropItemData = new DroppedItemData()
        {
            DropDirection = dropDirection,
            Position = position,
            UsedItem = usedItem,
            Player = player,
            Logger = logger,
            ItemRConfig = config,
            TimerThread = timerThread
        };

        timerThread.RunDelayed(DropItem, dropItemData, TimeSpan.FromMilliseconds(1000));
    }

    private class DroppedItemData : PlayerRoomTimer
    {
        public int DropDirection { get; set; }
        public ItemDescription UsedItem { get; set; }
        public Vector3 Position { get; set; }
        public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
        public ItemRConfig ItemRConfig { get; set; }
        public TimerThread TimerThread { get; set; }
    }

    private static void DropItem(ITimerData data)
    {
        if (data is not DroppedItemData drop)
            return;

        var player = drop.Player;

        var dropItem = new LaunchItem_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
            player.TempData.Position.x + drop.DropDirection, player.TempData.Position.y, player.TempData.Position.z,
            0, 0, 3, 0, drop.UsedItem.PrefabName);

        player.Room.SendSyncEvent(dropItem);

        var bombData = new BombData()
        {
            Position = player.TempData.CopyPosition(),
            Radius = 5.4f,
            Thread = drop.TimerThread,
            Damage = drop.UsedItem.GetDamageAmount(drop.Logger, drop.ItemRConfig),
            DamageType = drop.UsedItem.Elemental,
            Player = player,
        };

        drop.TimerThread.RunDelayed(ExplodeBomb, bombData, TimeSpan.FromMilliseconds(2850));
    }

    private class BombData : PlayerRoomTimer
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public int Damage { get; set; }
        public Elemental DamageType { get; set; }
        public TimerThread Thread { get; set; }
        public ServerRConfig ServerRConfig { get; set; }
    }

    private static void ExplodeBomb(ITimerData data)
    {
        if (data is not BombData bomb)
            return;

        ExplodeBomb(bomb.Player.Room, bomb.Player, bomb.Position, bomb.Radius, bomb.Damage, bomb.DamageType, bomb.ServerRConfig, bomb.Thread);
    }

    public static void ExplodeBomb(this Room room, Player player, Vector3 position,
        float radius, int damage, Elemental damageType, ServerRConfig serverRConfig, TimerThread thread)
    {
        foreach (var component in room.GetEntitiesFromType<BreakableEventControllerComp>().Where(comp =>
            Vector3.Distance(position, new Vector3(comp.Position.X, comp.Position.Y, comp.Position.Z)) <= radius
        ))
            component.Damage(damage, damageType, player);

        if (player == null)
            foreach (var nearPlayer in room.GetNearbyPlayers(position, radius))
                nearPlayer.ApplyCharacterDamage(damage, nearPlayer.GameObjectId, 1, serverRConfig, thread);

        room.Logger.LogInformation("Running bomb at coords: {Position} of radius {Radius}", position, radius);
    }

    public static int GetDamageAmount(this ItemDescription usedItem, Microsoft.Extensions.Logging.ILogger logger, ItemRConfig config)
    {
        var damage = config.DefaultDropDamage;

        if (usedItem.ItemEffects.Count == 0)
        {
            logger.LogWarning("Item ({usedItemName}) has 0 ItemEffects! Are you sure this item was set up correctly?", usedItem.ItemName);
            return damage;
        }

        foreach (var effect in usedItem.ItemEffects)
        {
            switch (effect.Type)
            {
                case ItemEffectType.BluntDamage:
                case ItemEffectType.FireDamage:
                case ItemEffectType.PoisonDamage:
                case ItemEffectType.IceDamage:
                case ItemEffectType.AirDamage:
                case ItemEffectType.EarthDamage:
                case ItemEffectType.WaterDamage:
                case ItemEffectType.LightningDamage:
                case ItemEffectType.StompDamage:
                case ItemEffectType.ArmorPiercingDamage:
                    damage = effect.Value;
                    break;
                default:
                    break;
            }

            logger.LogInformation("Item ({usedItemName}) with ({damageType}) has been used!", usedItem.ItemName, effect.Type);
        }
        return damage;
    }

    public static void VoteForItem(this Player player, int objectId, bool accepted)
    {
        player.TempData.VotedForItem.Add(objectId, accepted);

        var playersInRoom = player.Room.GetPlayers();

        foreach (var roomPlayer in playersInRoom)
            roomPlayer.SendXt(accepted ? "jr" : "jp", player.UserId, objectId);

        if (playersInRoom.All(x => x.TempData.VotedForItem.ContainsKey(objectId)))
        {
            var participants = playersInRoom.Where(x => x.TempData.VotedForItem[objectId] == true).ToList();
            var winningPlayer = participants[new Random().Next(participants.Count)];

            foreach (var roomPlayer in playersInRoom)
            {
                var rewardedData = new SeparatedStringBuilder('|');

                rewardedData.Append(objectId);
                rewardedData.Append(winningPlayer.UserId);

                roomPlayer.SendXt("jl", rewardedData.ToString());
                roomPlayer.SendUpdatedInventory();
                roomPlayer.TempData.VotedForItem.Remove(objectId);
            }
        }
    }
}
