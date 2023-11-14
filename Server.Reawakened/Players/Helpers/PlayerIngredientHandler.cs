using A2m.Server;
using Achievement.PackagedData;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Models.RecipeRewards;
using UnityEngine;

namespace Server.Reawakened.Players.Helpers;

public static class PlayerIngredientHandler
{
    public static void GrantRecipe(this Player player, int recipeId, int parentRecipeId,
        RecipeCatalogInt recipeCatalog, Microsoft.Extensions.Logging.ILogger logger)
    {
        var loot = recipeCatalog.GetRecipeById(recipeId);

        if (loot.RecipeId <= 0)
            logger.LogError("Recipe not yet implemented for recipe with ID '{RecipeId}'.", loot.RecipeId);

        if (loot.RecipeRewards.Count > 0)
            loot.RecipeRewards.GrantRecipeItem(parentRecipeId, player);
    }

    public static void GrantRecipeItem(this List<RecipeReward> recipeModels, int parentRecipeId,
        Player player) => player.SendXt("cz", GenerateRecipeData(recipeModels, parentRecipeId, player));

    private static string GenerateRecipeData(List<RecipeReward> recipeModels,
        int parentRecipeId, Player player)
    {
        var recipeData = "";

        foreach (var recipes in recipeModels)
        {
            foreach (var recipe in recipes.Recipes)
            {
                player.Character.Data.RecipeList.RecipeList.Add(recipe);

                player.SendCharacterInfoDataTo(player, Rooms.Enums.CharacterInfoType.Detailed, player.Room.LevelInfo);

                recipe.ItemId = parentRecipeId;
                recipeData = recipe.ToString();
            }
        }
        return recipeData.ToString();
    }
}
