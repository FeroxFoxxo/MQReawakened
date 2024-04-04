using Microsoft.Extensions.Logging;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalRecipe : InternalXml
{
    public override string BundleName => "InternalRecipe";
    public override BundlePriority Priority => BundlePriority.Low;

    public ILogger<InternalRecipe> Logger { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    public Dictionary<int, RecipeModel> RecipeCatalog;
    public Dictionary<string, List<int>> RecipeTypeList;

    public override void InitializeVariables()
    {
        RecipeCatalog = [];
        RecipeTypeList = [];
    }

    public RecipeModel GetRecipeById(int recipeId) =>
        RecipeCatalog.TryGetValue(recipeId, out var recipeInfo) ? recipeInfo : RecipeCatalog[0];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
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

                    var itemList = recipeInfo.GetXmlItems(ItemCatalog, Logger);

                    foreach (var item in itemList)
                    {
                        var ingredientModel = new IngredientModel()
                        {
                            ItemId = item.ItemId,
                            Count = item.Count
                        };

                        ingredients.Add(ingredientModel);
                    }

                    var recipeItem = ItemCatalog.GetItemFromPrefabName(recipeName);

                    if (recipeItem == null)
                    {
                        Logger.LogError("Could not find recipe with name: '{RecipeName}'", recipeName);
                        continue;
                    }

                    var itemId = recipeItem.RecipeParentItemID;

                    if (itemId <= 0)
                    {
                        Logger.LogError("Could not find parent item '{Parent}' for recipe name: '{RecipeName}'", itemId, recipeName);
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
}
