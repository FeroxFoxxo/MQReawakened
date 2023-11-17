using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class RecipeCatalogInt : IBundledXml
{
    public string BundleName => "RecipeCatalogInt";
    public BundlePriority Priority => BundlePriority.Low;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, RecipeModel> RecipeCatalog;

    public void InitializeVariables() => RecipeCatalog = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var itemCatalog = Services.GetRequiredService<ItemCatalog>();

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode recipeCatalog in xmlDocument.ChildNodes)
        {
            if (recipeCatalog.Name != "RecipeCatalog") continue;

            foreach (XmlNode recipeType in recipeCatalog.ChildNodes)
            {
                if (recipeType.Name != "RecipeType") continue;

                foreach (XmlNode recipeInfo in recipeType.ChildNodes)
                {
                    if (recipeInfo.Name != "RecipeInfo") continue;

                    var recipeId = -1;
                    var ingredients = new List<IngredientModel>();

                    foreach (XmlAttribute recipeAttribute in recipeInfo.Attributes)
                    {
                        switch (recipeAttribute.Name)
                        {
                            case "recipeId":
                                recipeId = int.Parse(recipeAttribute.Value);
                                break;
                        }
                    }

                    foreach (XmlNode ingredient in recipeInfo.ChildNodes)
                    {
                        switch (ingredient.Name)
                        {
                            case "Item":
                                var ingredientItem = -1;
                                var ingredientAmount = -1;

                                foreach (XmlAttribute item in ingredient.Attributes)
                                {
                                    switch (item.Name)
                                    {
                                        case "itemId":
                                            ingredientItem = int.Parse(item.Value);
                                            break;
                                        case "count":
                                            ingredientAmount = int.Parse(item.Value);
                                            break;
                                    }
                                }

                                var ingredientModel = new IngredientModel()
                                {
                                    ItemId = ingredientItem,
                                    Count = ingredientAmount
                                };

                                ingredients.Add(ingredientModel);

                                break;
                        }
                    }

                    if (!itemCatalog.Items.TryGetValue(recipeId, out var recipeItem))
                    {
                        Logger.LogError("Could not find recipe with id: {RecipeId}", recipeId);
                        continue;
                    }

                    var itemId = recipeItem.RecipeParentItemID;

                    if (itemId <= 0)
                    {
                        Logger.LogError("Could not find parent item {Parent} for recipe id: {RecipeId}", itemId, recipeId);
                        continue;
                    }

                    var recipeModel = new RecipeModel()
                    {
                        RecipeId = recipeId,
                        ItemId = itemId,
                        Ingredients = ingredients
                    };

                    RecipeCatalog.TryAdd(recipeId, recipeModel);
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public RecipeModel GetRecipeById(int recipeId) =>
        RecipeCatalog.TryGetValue(recipeId, out var recipeInfo) ? recipeInfo : RecipeCatalog[0];
}
