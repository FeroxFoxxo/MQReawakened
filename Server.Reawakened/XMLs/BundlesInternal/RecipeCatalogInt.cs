using A2m.Server;
using Achievement.PackagedData;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using Server.Reawakened.XMLs.Models.RecipeRewards;
using System.Collections.Generic;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class RecipeCatalogInt : IBundledXml
{
    public string BundleName => "RecipeCatalogInt";
    public BundlePriority Priority => BundlePriority.Low;
    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }
    public Dictionary<int, RecipeRewardModel> RecipeCatalog;
    public void InitializeVariables() => RecipeCatalog = [];
    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
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
                    var parentRecipeId = -1;
                    var recipeRewards = new List<RecipeReward>();
                    var recipes = new List<RecipeModel>();
                    var num = 0;

                    foreach (XmlAttribute recipeAttributes in recipeInfo.Attributes)
                    {
                        switch (recipeAttributes.Name)
                        {
                            case "recipeId":
                                if (recipeId == 0)
                                {
                                    Logger.LogWarning("RecipeId has an invalid id of {Id}, and does not exist within the RecipeCatalogInternal XML!", recipeId);
                                    continue;
                                }
                                else
                                    recipeId = int.Parse(recipeAttributes.Value);
                                break;
                            case "parentRecipeId":
                                if (parentRecipeId == 0)
                                    continue;
                                else
                                    parentRecipeId = int.Parse(recipeAttributes.Value);
                                break;
                        }
                    }

                    foreach (XmlNode ingredient in recipeInfo.ChildNodes)
                    {
                        num++;
                        switch (ingredient.Name)
                        {
                            case "Item":
                                recipes = recipeInfo.GetXmlRecipe(recipeId, parentRecipeId);
                                break;
                            default:
                                Logger.LogWarning("Unknown recipe type for recipeId {Id}", recipeId);
                                break;
                        }
                    }
                    var ingredientAmount = num;
                    recipeRewards.Add(new RecipeReward(recipes, ingredientAmount));

                    RecipeCatalog.TryAdd(recipeId, new RecipeRewardModel(recipeId, recipeRewards));
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public RecipeRewardModel GetRecipeById(int objectId) =>
        RecipeCatalog.TryGetValue(objectId, out var lootInfo) ? lootInfo : RecipeCatalog[0];
}
