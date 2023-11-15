using A2m.Server;
using Achievement.PackagedData;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using UnityEngine;

namespace Server.Reawakened.Players.Helpers;

public static class PlayerIngredientHandler
{
    public static void GrantRecipe(this Player player, int recipeId, int parentRecipeId,
        RecipeCatalogInt recipeCatalog, Microsoft.Extensions.Logging.ILogger logger)
    {
        var recipe = recipeCatalog.GetRecipeById(recipeId);

        if (recipeId <= 0)
            logger.LogError("Recipe not yet implemented for recipe with ID '{RecipeId}'.", recipeId);

        if (recipeId > 0)
            recipe.GrantRecipeItem(recipeId, parentRecipeId, player);
    }

    public static void GrantRecipeItem(this List<IngredientModel> ingredients, int recipeId, int parentRecipeId,
        Player player) => player.SendXt("cz", GenerateRecipeData(ingredients, recipeId, parentRecipeId, player));

    private static string GenerateRecipeData(List<IngredientModel> ingredients,
        int recipeId, int parentRecipeId, Player player)
    {
        var recipe = new RecipeModel()
        {
            RecipeId = recipeId,
            ItemId = parentRecipeId,
            Ingredients = ingredients
        };
        player.Character.Data.RecipeList.RecipeList.Add(recipe);

        player.SendCharacterInfoDataTo(player, Rooms.Enums.CharacterInfoType.Detailed, player.Room.LevelInfo);

        var recipeData = recipe.ToString();
        return recipeData.ToString();
    }
}
