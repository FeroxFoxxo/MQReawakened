using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using System.Linq;

namespace Protocols.External._h__HotbarHandler;
public class UseSlot : ExternalProtocol
{
    public override string ProtocolName => "hu";

    public ItemCatalog ItemCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public ILogger<PlayerStatus> Logger { get; set; }

    public override void Run(string[] message)
    {
        var hotbarSlotId = int.Parse(message[5]);
        var targetUserId = int.Parse(message[6]);
        var position = new Vector3Model()
        {
            X = Convert.ToSingle(message[7]),
            Y = Convert.ToSingle(message[8]),
            Z = Convert.ToSingle(message[9])
        };
        Logger.LogDebug("Player used hotbar slot {hotbarId} on {userId} at coordinates {position}",
            hotbarSlotId, targetUserId, position);
        var direction = Player.TempData.Direction;
        var slotItem = Player.Character.Data.Hotbar.HotbarButtons[hotbarSlotId];
        var usedItem = ItemCatalog.GetItemFromId(slotItem.ItemId);
        switch (usedItem.ItemActionType)
        {
            case ItemActionType.Drop:
                HandleDrop(usedItem, position, direction);
                break;
            case ItemActionType.Throw:
                HandleRangedWeapon(usedItem, position, direction);
                break;
            case ItemActionType.Genericusing:
            case ItemActionType.Drink:
            case ItemActionType.Eat:
                HandleConsumable(usedItem, TimerThread, ServerRConfig, hotbarSlotId, Logger);
                break;
            case ItemActionType.Melee:
                HandleMeleeWeapon(usedItem, position, direction);
                break;
            case ItemActionType.Pet:
                HandlePet(usedItem);
                break;
            case ItemActionType.Relic:
                HandleRelic(usedItem);
                break;
            default:
                Logger.LogError("Could not find how to handle item action type {ItemAction} for user {UserId}",
                    usedItem.ItemActionType, targetUserId);
                break;
        }
    }

    private void HandlePet(ItemDescription usedItem)
    {
        Player.SendXt("ZE", Player.UserId, usedItem.ItemId, 1);
        Player.Character.Data.PetItemId = usedItem.ItemId;
    }

    private void HandleRelic(ItemDescription usedItem) //Needs rework.
    {
        StatusEffect_SyncEvent itemEffect = null;
        foreach (var effect in usedItem.ItemEffects)
            itemEffect = new StatusEffect_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                    (int)effect.Type, effect.Value, effect.Duration, true, usedItem.PrefabName, false);
        Player.SendSyncEventToPlayer(itemEffect);
    }

    private void HandleDrop(ItemDescription usedItem, Vector3Model position, int direction)
    {
        var isLeft = direction > 0;
        var dropDirection = isLeft ? 1 : -1;
        var platform = new GameObjectModel();
        var planeName = position.Z > 10 ? ServerRConfig.IsBackPlane[true] : ServerRConfig.IsBackPlane[false];
        var dropItemData = new DroppedItemData()
        {
            DropDirection = dropDirection,
            Position = position,
            UsedItem = usedItem
        };
        TimerThread.DelayCall(DropItem, dropItemData, TimeSpan.FromMilliseconds(1000), TimeSpan.Zero, 1);
    }
    private class DroppedItemData()
    {
        public int DropDirection { get; set; }
        public ItemDescription UsedItem { get; set; }
        public Vector3Model Position { get; set; }
    }

    private void DropItem(object data)
    {
        var dropData = (DroppedItemData)data;
        var dropItem = new LaunchItem_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
            Player.TempData.Position.X + dropData.DropDirection, Player.TempData.Position.Y, Player.TempData.Position.Z,
            0, 0, 3, 0, dropData.UsedItem.PrefabName);
        Player.Room.SendSyncEvent(dropItem);
        foreach (var entity in Player.Room.Entities)
        {
            foreach (var component in entity.Value
                .Where(comp => Vector3Model.Distance(dropData.Position, comp.Position) <= 5.4f))
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
                        Damage = GetDamageType(dropData.UsedItem)
                    };
                    TimerThread.DelayCall(ExplodeBomb, bombData, TimeSpan.FromMilliseconds(2850), TimeSpan.Zero, 1);
                }
            }
        }
    }

    private int GetDamageType(ItemDescription usedItem)
    {
        var damage = ServerRConfig.DefaultDamage;
        if (usedItem.ItemEffects.Count == 0)
        {
            Logger.LogWarning("Item ({usedItemName}) has 0 ItemEffects! Are you sure this item was set up correctly?", usedItem.ItemName);
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
            Logger.LogInformation("Item ({usedItemName}) with ({damageType}) has been used!", usedItem.ItemName, effect.Type);
        }
        return damage;
    }

    private class BombData()
    {
        public string PrefabName { get; set; }
        public string ObjectId { get; set; }
        public BaseComponent Component { get; set; }
        public int Damage { get; set; }
    }

    private void ExplodeBomb(object data)
    {
        var bData = (BombData)data;
        Logger.LogInformation("Found close hazard {PrefabName} with Id {ObjectId}", bData.PrefabName, bData.ObjectId);
        if (bData.Component is BreakableEventControllerComp breakableObjEntity)
            breakableObjEntity.Damage(5, Player);
    }

    private void HandleConsumable(ItemDescription usedItem, TimerThread timerThread, ServerRConfig serverRConfig, int hotbarSlotId, ILogger<PlayerStatus> logger)
    {
        Player.HandleItemEffect(usedItem, timerThread, serverRConfig, logger);
        var removeFromHotbar = true;
        
        if (usedItem.InventoryCategoryID is
            ItemFilterCategory.WeaponAndAbilities or
            ItemFilterCategory.Pets)
            removeFromHotbar = false;
            
        if (removeFromHotbar)
        {
            if (usedItem.ItemActionType == ItemActionType.Eat)
            {
                Player.CheckAchievement(AchConditionType.Consumable, string.Empty, Logger);
                Player.CheckAchievement(AchConditionType.Consumable, usedItem.PrefabName, Logger);
            } 
            else if (usedItem.ItemActionType == ItemActionType.Drink)
            {
                Player.CheckAchievement(AchConditionType.Drink, string.Empty, Logger);
                Player.CheckAchievement(AchConditionType.Drink, usedItem.PrefabName, Logger);
            }

            RemoveFromHotbar(Player.Character, usedItem, hotbarSlotId);
        }
    }

    private void HandleRangedWeapon(ItemDescription usedItem, Vector3Model position, int direction)
    {
        var rand = new Random();
        var prjId = Math.Abs(rand.Next()).ToString();
        while (Player.Room.GameObjectIds.Contains(prjId))
            prjId = Math.Abs(rand.Next()).ToString();

        // Magic number 10 is damage for now, until we add a serverside stat handler
        var prj = new ProjectileEntity(Player, prjId, position, direction, 3, usedItem, 10, usedItem.Elemental, ServerRConfig);

        Player.Room.Projectiles.Add(prjId, prj);
    }

    private void HandleMeleeWeapon(ItemDescription usedItem, Vector3Model position, int direction)
    {
        var rand = new Random();
        var prjId = Math.Abs(rand.Next()).ToString(); ;
        while (Player.Room.GameObjectIds.Contains(prjId))
            prjId = Math.Abs(rand.Next()).ToString();

        // Magic number 10 is damage for now, until we add a serverside stat handler
        var prj = new MeleeEntity(Player, prjId, position, direction, 3, usedItem, 10, usedItem.Elemental, ServerRConfig);

        Player.Room.Projectiles.Add(prjId, prj);
    }

    private void RemoveFromHotbar(CharacterModel character, ItemDescription item, int hotbarSlotId)
    {
        character.Data.Inventory.Items[item.ItemId].Count--;
        if (character.Data.Inventory.Items[item.ItemId].Count <= 0)
        {
            character.Data.Hotbar.HotbarButtons.Remove(hotbarSlotId);
            SendXt("hu", character.Data.Hotbar);
        }
        Player.SendUpdatedInventory(false);
    }
}
