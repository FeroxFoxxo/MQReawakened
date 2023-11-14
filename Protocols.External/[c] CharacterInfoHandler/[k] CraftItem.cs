using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._c__CharacterInfoHandler;
public class CraftItem : ExternalProtocol
{
    public override string ProtocolName => "ck";

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var recipeId = int.Parse(message[5]);
        var amount = message[6] == "1";

        var recipeDescription = ItemCatalog.GetItemFromId(recipeId);
        var recipeParentDescription = ItemCatalog.GetItemFromId(recipeDescription.RecipeParentItemID);

        Player.Character.AddItem(recipeParentDescription, 1);

        foreach (var recipe in Player.Character.Data.RecipeList.RecipeList)
            if (recipe.RecipeId == recipeId)
                foreach (var ingredient in recipe.Ingredients)
                    Player.Character.RemoveItem(ItemCatalog.GetItemFromId(ingredient.ItemId), ingredient.Count);

        Player.SendUpdatedInventory(false);

        SendXt("ck", recipeId, amount);
    }
}
