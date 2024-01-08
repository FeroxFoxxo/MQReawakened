using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Protocols.External._i__InventoryHandler;

public class UseItem : ExternalProtocol
{
    public override string ProtocolName => "iu";

    public ILogger<UseItem> Logger { get; set; }

    public VendorCatalog VendorCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public RecipeCatalogInt RecipeCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var itemId = int.Parse(message[5]);
        var item = ItemCatalog.GetItemFromId(itemId);

        if (item == null)
        {
            Logger.LogError("Could not find item with id {ItemId}", itemId);
            return;
        }

        character.RemoveItem(item, 1);

        switch (item.SubCategoryId)
        {
            case ItemSubCategory.Potion:
                StatusEffect_SyncEvent statusEffect = null;

                foreach (var effect in item.ItemEffects)
                {
                    if (effect.Type is ItemEffectType.Invalid or ItemEffectType.Unknown)
                        return;

                    if (effect.Type is ItemEffectType.Healing)
                        Player.HealCharacter(item, ServerRConfig);

                    statusEffect = new StatusEffect_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                        effect.TypeId, effect.Value, effect.Duration, true, Player.GameObjectId.ToString(), true);
                }
                Player.SendSyncEventToPlayer(statusEffect);

                var removeFromHotbar = true;

                if (item.ItemId == ServerRConfig.HealingStaff) //Prevents Healing Staff from removing itself.
                    removeFromHotbar = false;

                if (!item.UniqueInInventory && removeFromHotbar)
                    RemoveFromHotbar(Player.Character, item);
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

                    character.AddItem(packItem, pair.Value);
                }
                break;
            default:
                Logger.LogWarning("Could not find use for item {ItemId}, type {ItemType}.",
                    itemId, item.SubCategoryId);
                break;
        }

        Player.SendUpdatedInventory(false);
    }

    private void RemoveFromHotbar(CharacterModel character, ItemDescription item)
    {
        character.Data.Inventory.Items[item.ItemId].Count--;

        if (character.Data.Inventory.Items[item.ItemId].Count <= 0)
        {
            if (character.Data.Inventory.Items[item.ItemId] != null)
                character.Data.Inventory.Items.Remove(item.ItemId);

            character.Data.Inventory.Items[item.ItemId].Count = -1;
        }

        Player.SendUpdatedInventory(false);
    }
}
