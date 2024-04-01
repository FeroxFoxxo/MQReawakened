using Achievement.Categories;
using Achievement.StaticData;
using Achievement.Types;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Icons.Services;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;
public class InternalAchievement : InternalXml
{
    public override string BundleName => "InternalAchievement";
    public override BundlePriority Priority => BundlePriority.Lowest;

    public ILogger<InternalAchievement> Logger { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ExtractIcons ExtractIcons { get; set; }

    public AchievementStaticJson.AchievementDefinition Definitions { get; private set; }
    public Dictionary<int, List<string>> PossibleConditions { get; private set; }

    public override void InitializeVariables()
    {
        Definitions = new AchievementStaticJson.AchievementDefinition()
        {
            status = true,
            achievements = [],
            categories = [],
            characterTitles = [], // UNUSED
            types = new AchievementType
            {
                conditions = [], // UNUSED
                rewards = [], // ONLY ID IS USED
                timeWindows = [] // UNUSED
            }
        };

        PossibleConditions = [];
    }

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        var enumValues = Enum.GetValues<RewardType>();

        foreach (var value in enumValues)
        {
            var enumName = Enum.GetName(value);

            var reward = new AchievementRewardType()
            {
                id = (int)value,
                title = enumName
            };

            Definitions.types.rewards.Add(reward);
        }

        var cIds = new List<int>();
        var aIds = new List<int>();

        foreach (XmlNode achievementXml in xmlDocument.ChildNodes)
        {
            if (achievementXml.Name != "Achievements") continue;

            foreach (XmlNode category in achievementXml.ChildNodes)
            {
                if (category.Name != "Category") continue;

                var categoryId = -1;
                var categoryTitle = string.Empty;
                var categoryIconName = string.Empty;
                var categorySortOrder = -1;

                foreach (XmlAttribute categoryAttribute in category.Attributes)
                    switch (categoryAttribute.Name)
                    {
                        case "id":
                            categoryId = int.Parse(categoryAttribute.Value);
                            continue;
                        case "title":
                            categoryTitle = categoryAttribute.Value;
                            continue;
                        case "iconName":
                            categoryIconName = categoryAttribute.Value;
                            continue;
                        case "sortOrder":
                            categorySortOrder = int.Parse(categoryAttribute.Value);
                            continue;
                    }

                if (cIds.Contains(categoryId))
                {
                    Logger.LogError("Category with id of {catId} already exists!", categoryId);
                    return;
                }

                cIds.Add(categoryId);

                var categoryType = new AchievementCategoryType()
                {
                    id = categoryId,
                    title = categoryTitle,
                    iconName = categoryIconName,
                    sortOrder = categorySortOrder
                };

                var categoryCount = 0;

                foreach (XmlNode achievement in category.ChildNodes)
                {
                    if (achievement.Name != "Achievement") continue;

                    var achId = -1;
                    var achTitle = string.Empty;
                    var achDescription = string.Empty;
                    var achIconName = string.Empty;
                    var achPoints = -1;
                    var achSortOrder = -1;
                    var achRepeatable = false;
                    var achRewards = new List<AchievementDefinitionRewards>();
                    var achConditions = new List<AchievementDefinitionConditions>();

                    foreach (XmlAttribute achievementAttribute in achievement.Attributes)
                        switch (achievementAttribute.Name)
                        {
                            case "id":
                                achId = int.Parse(achievementAttribute.Value);
                                continue;
                            case "title":
                                achTitle = achievementAttribute.Value;
                                continue;
                            case "description":
                                achDescription = achievementAttribute.Value;
                                continue;
                            case "iconName":
                                achIconName = achievementAttribute.Value;
                                continue;
                            case "points":
                                achPoints = int.Parse(achievementAttribute.Value);
                                continue;
                            case "sortOrder":
                                achSortOrder = int.Parse(achievementAttribute.Value);
                                continue;
                            case "repeatable":
                                achRepeatable = achRepeatable.GetBoolValue(achievementAttribute.Value, Logger);
                                continue;
                        }

                    foreach (XmlNode achievementLists in achievement.ChildNodes)
                    {
                        switch (achievementLists.Name)
                        {
                            case "Rewards":
                                achRewards = achievementLists.GetXmlRewards(Logger, ItemCatalog, achId);
                                break;
                            case "Conditions":
                                achConditions = achievementLists.GetXmlConditions(Logger, achId);
                                break;
                        }
                    }

                    if (!ExtractIcons.HasIcon($"ACH_{achIconName}_ON"))
                        continue;

                    if (aIds.Contains(achId))
                    {
                        Logger.LogError("Achievement with id of {achId} already exists!", achId);
                        return;
                    }

                    aIds.Add(achId);

                    var achievementStatic = new AchievementStaticData()
                    {
                        id = achId,
                        categoryId = categoryId,
                        title = achTitle,
                        description = achDescription,
                        iconName = achIconName,
                        points = achPoints,
                        sortOrder = achSortOrder,
                        repeatable = achRepeatable,
                        rewards = achRewards,
                        conditions = achConditions,
                        goal = achConditions.Sum(c => c.goal)
                    };

                    Definitions.achievements.Add(achievementStatic);
                    categoryCount++;
                }

                if (categoryCount > 0 || categoryId <= 1)
                    Definitions.categories.Add(categoryType);
            }
        }

        var currentCount = 0;

        foreach (var category in Definitions.categories.OrderBy(x => x.sortOrder))
        {
            var loopedAchievements = Definitions.achievements.Where(x => x.categoryId == category.id);

            currentCount += loopedAchievements.Any() ? loopedAchievements.Max(x => x.id) : 0;

            foreach (var achievement in loopedAchievements)
            {
                achievement.sortOrder = currentCount - achievement.sortOrder;

                foreach (var cond in achievement.conditions)
                {
                    var type = (AchConditionType)cond.typeId;

                    if (type is AchConditionType.Unknown or AchConditionType.Invalid)
                        continue;

                    if (!PossibleConditions.ContainsKey(cond.typeId))
                        PossibleConditions.Add(cond.typeId, []);

                    if (!PossibleConditions[cond.typeId].Contains(cond.description))
                        PossibleConditions[cond.typeId].Add(cond.description);
                }
            }
        }
    }
}
