using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Protocols.External._c__CharacterInfoHandler;

public class CraftItem : ExternalProtocol
{
    public override string ProtocolName => "ck";

    public ItemCatalog ItemCatalog { get; set; }
    public RecipeCatalogInt RecipeCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ILogger<CraftItem> Logger { get; set; }

    public override void Run(string[] message)
    {
        var recipeId = int.Parse(message[5]);

        if (!RecipeCatalog.RecipeCatalog.TryGetValue(recipeId, out var recipe))
        {
            Logger.LogError("Recipe with id {RecipeId} does not exist!", recipeId);
            return;
        }

        var amount = ServerRConfig.Is2014Client
            ? int.Parse(message[6])
            : message[6].Equals("true", StringComparison.CurrentCultureIgnoreCase) // should craft all
                ? recipe.Ingredients.Min(ing => Player.Character.TryGetItem(ing.ItemId, out var pItem) ? 0 : pItem.Count / ing.Count)
                : 1;

        if (!ItemCatalog.Items.TryGetValue(recipe.ItemId, out var item))
        {
            Logger.LogError("Could not find recipe item with id: {ItemId}", recipe.ItemId);
            return;
        }

        Player.Character.AddItem(item, amount);

        foreach (var ingredient in recipe.Ingredients)
        {
            var ingredientItem = ItemCatalog.GetItemFromId(ingredient.ItemId);
            Player.Character.RemoveItem(ingredientItem, ingredient.Count * amount);
        }

        Player.SendUpdatedInventory(false);

        SendXt("ck", recipeId, amount);
    }
}
