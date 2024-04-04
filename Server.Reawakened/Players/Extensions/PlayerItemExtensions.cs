using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Components.GameObjects.Hazards;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Players.Extensions;
public static class PlayerItemExtensions
{
    public static void HandleDrop(this Player player, ItemRConfig config, TimerThread timerThread,
        Microsoft.Extensions.Logging.ILogger logger, ItemDescription usedItem, Vector3Model position, int direction)
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

        timerThread.DelayCall(DropItem, dropItemData, TimeSpan.FromMilliseconds(1000), TimeSpan.Zero, 1);
    }

    private class DroppedItemData()
    {
        public int DropDirection { get; set; }
        public ItemDescription UsedItem { get; set; }
        public Vector3Model Position { get; set; }
        public Player Player { get; set; }
        public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
        public ItemRConfig ItemRConfig { get; set; }
        public TimerThread TimerThread { get; set; }
    }

    private static void DropItem(object data)
    {
        var dropData = (DroppedItemData)data;
        var player = dropData.Player;

        if (player == null)
            return;

        if (player.Character == null || player.Room == null || player.TempData == null)
            return;

        var dropItem = new LaunchItem_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
            player.TempData.Position.X + dropData.DropDirection, player.TempData.Position.Y, player.TempData.Position.Z,
            0, 0, 3, 0, dropData.UsedItem.PrefabName);

        player.Room.SendSyncEvent(dropItem);

        var components = new List<BaseComponent>();

        components.AddRange(player.Room.GetEntitiesFromType<HazardControllerComp>());
        components.AddRange(player.Room.GetEntitiesFromType<BreakableEventControllerComp>());

        //Needs collider-based rework.
        foreach (var component in components.Where(comp => Vector3Model.Distance(dropData.Position, comp.Position) <= 5.4f))
        {
            var prefabName = component.PrefabName;
            var objectId = component.Id;

            if (component is HazardControllerComp or BreakableEventControllerComp)
            {
                var bombData = new BombData()
                {
                    PrefabName = prefabName,
                    Component = component,
                    ObjectId = objectId,
                    Damage = dropData.UsedItem.GetDamageAmount(dropData.Logger, dropData.ItemRConfig),
                    DamageType = dropData.UsedItem.Elemental,
                    Player = player,
                    Logger = dropData.Logger
                };

                dropData.TimerThread.DelayCall(ExplodeBomb, bombData, TimeSpan.FromMilliseconds(2850), TimeSpan.Zero, 1);
            }
        }
    }

    private class BombData()
    {
        public string PrefabName { get; set; }
        public string ObjectId { get; set; }
        public BaseComponent Component { get; set; }
        public int Damage { get; set; }
        public Elemental DamageType { get; set; }
        public Player Player { get; set; }
        public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    }

    private static void ExplodeBomb(object data)
    {
        var bData = (BombData)data;

        bData.Logger.LogInformation("Found close hazard {PrefabName} with Id {ObjectId}", bData.PrefabName, bData.ObjectId);

        if (bData.Player == null)
            return;

        if (bData.Component is BreakableEventControllerComp breakableObjEntity)
            breakableObjEntity.Damage(bData.Damage, bData.DamageType, bData.Player);
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
}
