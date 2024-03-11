using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalRecipe : IBundledXml<InternalRecipe>
{
    public string BundleName => "InternalRecipe";
    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<InternalRecipe> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, RecipeModel> RecipeCatalog;
    public Dictionary<string, List<int>> RecipeTypeList;

    public void InitializeVariables()
    {
        RecipeCatalog = [];
        RecipeTypeList = [];
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var itemCatalog = Services.GetRequiredService<ItemCatalog>();

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode recipeXml in xmlDocument.ChildNodes)
        {
            if (recipeXml.Name != "RecipeCatalog") continue;

            foreach (XmlNode recipeType in recipeXml.ChildNodes)
            {
                if (recipeType.Name != "RecipeType") continue;

                var name = string.Empty;
                var typeList = new List<int>();

                foreach (XmlAttribute recipeTypeAttribute in recipeType.Attributes)
                {
                    switch (recipeTypeAttribute.Name)
                    {
                        case "name":
                            name = recipeTypeAttribute.Value;
                            break;
                    }
                }

                foreach (XmlNode recipeInfo in recipeType.ChildNodes)
                {
                    if (recipeInfo.Name != "RecipeInfo") continue;

                    var recipeName = string.Empty;
                    var ingredients = new List<IngredientModel>();

                    foreach (XmlAttribute recipeAttribute in recipeInfo.Attributes)
                    {
                        switch (recipeAttribute.Name)
                        {
                            case "recipeName":
                                recipeName = recipeAttribute.Value;
                                break;
                        }
                    }

                    var itemList = recipeInfo.GetXmlItems(itemCatalog, Logger);

                    foreach (var item in itemList)
                    {
                        var ingredientModel = new IngredientModel()
                        {
                            ItemId = item.ItemId,
                            Count = item.Count
                        };

                        ingredients.Add(ingredientModel);
                    }

                    var recipeItem = itemCatalog.GetItemFromPrefabName(recipeName);

                    if (recipeItem == null)
                    {
                        Logger.LogError("Could not find recipe with name: {RecipeName}", recipeName);
                        continue;
                    }

                    var itemId = recipeItem.RecipeParentItemID;

                    if (itemId <= 0)
                    {
                        Logger.LogError("Could not find parent item {Parent} for recipe name: {RecipeName}", itemId, recipeName);
                        continue;
                    }

                    var recipeModel = new RecipeModel()
                    {
                        RecipeId = recipeItem.ItemId,
                        ItemId = itemId,
                        Ingredients = ingredients
                    };

                    RecipeCatalog.TryAdd(recipeModel.ItemId, recipeModel);
                    typeList.Add(recipeModel.ItemId);
                }

                RecipeTypeList.TryAdd(name, typeList);
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public RecipeModel GetRecipeById(int recipeId) =>
        RecipeCatalog.TryGetValue(recipeId, out var recipeInfo) ? recipeInfo : RecipeCatalog[0];
}
