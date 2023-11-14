using A2m.Server;
using Server.Reawakened.Players.Models.Character;
using System.Xml;

public static class GetRecipe
{
    public static List<RecipeModel> GetXmlRecipe(this XmlNode item, int recipeId, int parentRecipeId)
    {
        var recipeList = new List<RecipeModel>();
        var ingredientList = new List<IngredientModel>();
        _ = new RecipeModel();
        _ = new IngredientModel();

        var ingItemId = 0;
        var ingItemCount = 0;

        foreach (XmlNode itemAttributes in item.ChildNodes)
        {
            switch (itemAttributes.Name)
            {
                case "Item":
                    foreach (XmlAttribute ingredient in itemAttributes.Attributes)
                    {
                        switch (ingredient.Name)
                        {
                            case "itemId":
                                ingItemId = int.Parse(ingredient.Value);
                                break;
                            case "count":
                                ingItemCount = int.Parse(ingredient.Value);
                                break;
                        }
                    }
                    var ingredientModel = new IngredientModel()
                    {
                        ItemId = ingItemId,
                        Count = ingItemCount
                    };
                    ingredientList.Add(ingredientModel);
                    break;
            }
        }

        var recipe = new RecipeModel()
        {
            RecipeId = recipeId,
            ItemId = parentRecipeId,
            Ingredients = ingredientList,
        };

        recipeList.Add(recipe);

        return recipeList;
    }
}
