using Achievement.StaticData;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;
using Server.Reawakened.XMLs.Data.LootRewards.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Abstractions.Extensions;

public static class GetInternalXml
{
    public static List<ItemModel> GetXmlItems(this XmlNode node, ItemCatalog itemCatalog, Microsoft.Extensions.Logging.ILogger logger) =>
        [.. node.GetXmlLootItems(itemCatalog, logger).Select(c => c.Value)];

    public static List<KeyValuePair<int, ItemModel>> GetXmlLootItems(this XmlNode node, ItemCatalog itemCatalog, Microsoft.Extensions.Logging.ILogger logger)
    {
        var itemList = new List<KeyValuePair<int, ItemModel>>();

        foreach (XmlNode item in node.ChildNodes)
        {
            if (item.Name != "Item")
                continue;

            var itemName = string.Empty;
            var count = -1;
            var bindingCount = -1;
            var delayUseExpiry = DateTime.Now;
            var weight = 1;

            foreach (XmlAttribute itemAttribute in item.Attributes)
                switch (itemAttribute.Name)
                {
                    case "itemName":
                        itemName = itemAttribute.Value;
                        break;
                    case "count":
                        count = int.Parse(itemAttribute.Value);
                        break;
                    case "bindingCount":
                        bindingCount = int.Parse(itemAttribute.Value);
                        break;
                    case "delayUseExpiry":
                        delayUseExpiry = DateTime.Parse(itemAttribute.Value);
                        break;
                    case "weight":
                        weight = int.Parse(itemAttribute.Value);
                        break;
                }

            var itemId = 0;

            if (!itemName.Equals("none", StringComparison.CurrentCultureIgnoreCase))
            {
                var itemModel = itemCatalog.GetItemFromPrefabName(itemName);

                if (itemModel == null)
                {
                    logger.LogError("Could not find item with name: '{ItemName}'", itemName);
                    continue;
                }

                itemId = itemModel.ItemId;

                if (!itemCatalog.CanAddItem(itemModel))
                    continue;
            }

            itemList.Add(
                new KeyValuePair<int, ItemModel>(
                    weight,
                    new ItemModel()
                    {
                        ItemId = itemId,
                        Count = count,
                        BindingCount = bindingCount,
                        DelayUseExpiry = delayUseExpiry,
                    }
                )
            );
        }

        return itemList;
    }

    public static List<AchievementDefinitionRewards> GetXmlRewards(this XmlNode node,
        Microsoft.Extensions.Logging.ILogger logger, ItemCatalog catalog, int achievementId = 0)
    {
        var rewardList = new List<AchievementDefinitionRewards>();
        var id = 0;

        foreach (XmlNode reward in node.ChildNodes)
        {
            if (reward.Name != "Reward")
                continue;

            var type = RewardType.Unknown;
            object value = null;
            var quantity = -1;

            foreach (XmlAttribute rewardAttribute in reward.Attributes)
                switch (rewardAttribute.Name)
                {
                    case "type":
                        type = type.GetEnumValue(rewardAttribute.Value, logger);
                        continue;
                    case "value":
                        value = int.TryParse(rewardAttribute.Value, out var valInt) ? valInt : rewardAttribute.Value;
                        continue;
                    case "quantity":
                        quantity = int.Parse(rewardAttribute.Value);
                        continue;
                }

            id++;

            if (type == RewardType.Item)
            {
                var item = catalog.GetItemFromPrefabName(value.ToString());

                if (item != null)
                    value = item.ItemId;
                else
                {
                    logger.LogError("Unknown item with prefab name: '{Name}'", value);
                    continue;
                }
            }

            rewardList.Add(new AchievementDefinitionRewards()
            {
                id = int.Parse(achievementId <= 0 ? id.ToString() : achievementId.ToString() + id),
                achievementId = achievementId,
                typeId = (int)type,
                value = value,
                quantity = quantity
            });
        }

        return rewardList;
    }

    public static List<AchievementDefinitionConditions> GetXmlConditions(this XmlNode node,
        Microsoft.Extensions.Logging.ILogger logger, int achievementId)
    {
        var conditionList = new List<AchievementDefinitionConditions>();

        foreach (XmlNode condition in node.ChildNodes)
        {
            if (condition.Name != "Condition")
                continue;

            var id = -1;
            var title = string.Empty;
            var goal = -1;
            var type = AchConditionType.Invalid;
            var strType = string.Empty;
            var value = string.Empty;
            var visible = false;

            foreach (XmlAttribute conditionAttribute in condition.Attributes)
                switch (conditionAttribute.Name)
                {
                    case "id":
                        id = int.Parse(conditionAttribute.Value);
                        continue;
                    case "title":
                        title = conditionAttribute.Value;
                        continue;
                    case "type":
                        strType = conditionAttribute.Value;
                        type = type.GetEnumValue(strType, logger);
                        continue;
                    case "value":
                        value = conditionAttribute.Value;
                        continue;
                    case "goal":
                        goal = int.Parse(conditionAttribute.Value);
                        continue;
                    case "visible":
                        visible = visible.GetBoolValue(conditionAttribute.Value, logger);
                        continue;
                }

            if (type == AchConditionType.Invalid)
                logger.LogError("Unknown condition '{Type}' for: '{Name}' " +
                    "(Ach Id: {Id}, Cond Id: {CId})", strType, title, achievementId, id);

            if (string.IsNullOrEmpty(value))
                value = "any";

            conditionList.Add(new AchievementDefinitionConditions()
            {
                id = int.Parse(achievementId.ToString() + id),
                achievementId = achievementId,
                title = title,
                goal = goal,
                visible = visible,
                typeId = (int)type,
                sortOrder = id,
                description = value.ToLower()
            });
        }

        return conditionList;
    }

    public static void RewardPlayer(this List<AchievementDefinitionRewards> rewards, Player player, InternalAchievement internalAchievement,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        var hasUpdatedItems = false;

        var itemCatalog = internalAchievement.ItemCatalog;
        var config = itemCatalog.Config;

        foreach (var reward in rewards)
            switch ((RewardType)reward.typeId)
            {
                case RewardType.NickCash:
                    var nickCash = int.Parse(reward.value.ToString());
                    player.AddNCash(nickCash);
                    break;
                case RewardType.Bananas:
                    var bananaCount = int.Parse(reward.value.ToString());
                    player.AddBananas(bananaCount, internalAchievement, logger);
                    break;
                case RewardType.Item:
                    var itemId = int.Parse(reward.value.ToString());
                    var quantity = reward.quantity;

                    var item = itemCatalog.GetItemFromId(itemId);

                    player.AddItem(item, quantity, itemCatalog);
                    hasUpdatedItems = true;
                    break;
                case RewardType.Xp:
                    var xp = int.Parse(reward.value.ToString());

                    player.AddReputation(xp, config);
                    break;
                case RewardType.Title:
                    break;
                default:
                    logger.LogError("Unknown reward type {Type}", reward.typeId);
                    break;
            }

        if (hasUpdatedItems)
            player.SendUpdatedInventory();
    }
}
