using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Extensions;
using Server.Base.Timers.Extensions;

namespace Protocols.External._i__InventoryHandler;

public class UseItem : ExternalProtocol
{
    public override string ProtocolName => "iu";

    public VendorCatalog VendorCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public InternalRecipe RecipeCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public ILogger<PlayerStatus> Logger { get; set; }

    public override void Run(string[] message)
    {
        var itemId = int.Parse(message[5]);
        var usedItem = ItemCatalog.GetItemFromId(itemId);

        if (usedItem == null)
        {
            Logger.LogError("Could not find item with id {ItemId}", itemId);
            return;
        }

        Player.RemoveItem(usedItem, 1);

        var position = Player.TempData.Position;
        var direction = Player.TempData.Direction;

        var subCategoryId = usedItem.SubCategoryId;

        switch (subCategoryId)
        {
            case ItemSubCategory.Bomb:
                HandleDrop(usedItem, position, direction);
                break;
            case ItemSubCategory.Nut:
            case ItemSubCategory.Usable:
            case ItemSubCategory.Liquid:
            case ItemSubCategory.Elixir:
            case ItemSubCategory.Potion:
                HandleConsumable(usedItem, TimerThread, ServerRConfig, Logger);
                break;
            case ItemSubCategory.Tailoring:
            case ItemSubCategory.Alchemy:
                var recipe = RecipeCatalog.GetRecipeById(itemId);

                Player.Character.Data.RecipeList.RecipeList.Add(recipe);

                Player.SendXt("cz", recipe);
                break;
            case ItemSubCategory.SuperPack:
                foreach (var pair in VendorCatalog.GetSuperPacksItemQuantityMap(itemId))
                {
                    var packItem = ItemCatalog.GetItemFromId(pair.Key);

                    if (packItem == null)
                        continue;

                    Player.AddItem(packItem, pair.Value);
                }
                break;
            default:
                Logger.LogWarning("Could not find use for item {ItemId}, type {ItemType}.",
                    itemId, usedItem.SubCategoryId);
                return;
        }

        Player.SendUpdatedInventory(false);
    }

    private void HandleConsumable(ItemDescription usedItem, TimerThread timerThread, ServerRConfig serverRConfig, ILogger<PlayerStatus> logger)
    {
        Player.HandleItemEffect(usedItem, timerThread, serverRConfig, logger);

        var removeFromHotbar = true;

        if (usedItem.InventoryCategoryID is
            ItemFilterCategory.WeaponAndAbilities or
            ItemFilterCategory.Pets)
            removeFromHotbar = false;

        if (removeFromHotbar)
            RemoveFromHotbar(Player.Character, usedItem);
    }

    private void HandleDrop(ItemDescription usedItem, Vector3Model position, int direction)
    {
        var isLeft = direction > 0;

        var dropDirection = isLeft ? 1 : -1;

        var platform = new GameObjectModel();

        var planeName = position.Z > 10 ? ServerRConfig.IsBackPlane[false] : ServerRConfig.IsBackPlane[true];
        position.Z = 0;

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
            Logger.LogWarning("Item ({usedItemName}) has 0 ItemEffects! Are you sure this item was set up correctly?", usedItem.PrefabName);
            return damage;
        }

        foreach (var effect in usedItem.ItemEffects)
        {
            switch (effect.Type)
            {    
                case ItemEffectType.FireDamage:
                case ItemEffectType.PoisonDamage:
                case ItemEffectType.IceDamage:
                case ItemEffectType.AirDamage:
                    damage = effect.Value;
                    break;
                default:
                    break;
            }
        }
        return damage;
    }
    
    private class BombData()
    {
        public string PrefabName { get; set; }
        public int ObjectId { get; set; }
        public BaseComponent Component { get; set; }
        public int Damage { get; set; }
    }

    private void ExplodeBomb(object data)
    {
        var bData = (BombData)data;

        Logger.LogInformation("Found close hazard {PrefabName} with Id {ObjectId}", bData.PrefabName, bData.ObjectId);

        if (bData.Component is BreakableEventControllerComp breakableObjEntity)
            breakableObjEntity.Destroy(Player);
        else if (bData.Component is InterObjStatusComp enemyEntity)
            enemyEntity.SendDamageEvent(Player, bData.Damage);
    }

    private void RemoveFromHotbar(CharacterModel character, ItemDescription item)
    {
        character.Data.Inventory.Items[item.ItemId].Count--;

        if (character.Data.Inventory.Items[item.ItemId].Count <= 0)
            if (character.Data.Inventory.Items[item.ItemId] != null)
                character.Data.Inventory.Items.Remove(item.ItemId);

        Player.SendUpdatedInventory(false);
    }
}
