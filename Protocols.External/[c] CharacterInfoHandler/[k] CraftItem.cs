using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;

namespace Protocols.External._c__CharacterInfoHandler;

public class CraftItem : ExternalProtocol
{
    public override string ProtocolName => "ck";

    public ItemCatalog ItemCatalog { get; set; }
    public InternalRecipe RecipeCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ILogger<CraftItem> Logger { get; set; }
    public InternalAchievement InternalAchievement { get; set; }

    public override void Run(string[] message)
    {
        var recipeId = int.Parse(message[5]);

        if (!ItemCatalog.Items.TryGetValue(recipeId, out var item))
        {
            Logger.LogError("Could not find recipe item with id: {ItemId}", item.ItemId);
            return;
        }

        if (!RecipeCatalog.RecipeCatalog.TryGetValue(item.RecipeParentItemID, out var recipe))
        {
            Logger.LogError("Recipe with id {RecipeId} does not exist!", recipeId);
            return;
        }

        var amount = ServerRConfig.GameVersion >= GameVersion.v2014
            ? int.Parse(message[6])
            : message[6].Equals("true", StringComparison.CurrentCultureIgnoreCase) // should craft all
                ? recipe.Ingredients.Min(ing => Player.Character.TryGetItem(ing.ItemId, out var pItem) ? 0 : pItem.Count / ing.Count)
                : 1;

        foreach (var ingredient in recipe.Ingredients)
        {
            var ingredientItem = ItemCatalog.GetItemFromId(ingredient.ItemId);
            Player.RemoveItem(ingredientItem, ingredient.Count * amount, ItemCatalog);
        }

        var itemDesc = ItemCatalog.GetItemFromId(recipe.ItemId);

        if (itemDesc != null)
        {
            Player.AddItem(itemDesc, amount, ItemCatalog);

            Player.CheckAchievement(AchConditionType.CraftItem, itemDesc.PrefabName, InternalAchievement, Logger);
        }

        Player.SendUpdatedInventory();

        SendXt("ck", recipeId, amount);
    }
}
