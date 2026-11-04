using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.XMLs.Data.Portals.Models;
public class PortalConditionModel
{
    public List<int> RequiredItems;
    public List<int> RequiredQuests;
    public int RequiredLevel;
    public int QuestConditionOp;
    public int ItemConditionOp;
    public ItemCatalog ItemCatalog;
    public QuestCatalog QuestCatalog;

    public PortalConditionModel(List<int> items, List<int> quests, int level, ItemCatalog itemCatalog, QuestCatalog questCatalog)
    {
        RequiredItems = items;
        RequiredQuests = quests;
        RequiredLevel = level;
        QuestConditionOp = 0;
        ItemConditionOp = 0;
        
        if (quests.Count == 1)
            QuestConditionOp = 1;
        else if (quests.Count > 1)
            QuestConditionOp = 2;

        if (items.Count == 1)
            ItemConditionOp = 1;
        else if (items.Count > 1)
            ItemConditionOp = 2;
        
        ItemCatalog = itemCatalog;
        QuestCatalog = questCatalog;
    }

    public bool CheckConditions(Player player)
    {
        var canUsePortal = CheckRequiredLevels(player);
        
        if (canUsePortal)
            canUsePortal = CheckRequiredItems(player);
        
        if (canUsePortal)
            canUsePortal = CheckRequiredQuests(player);
        
        return canUsePortal;
    }

    private bool CheckRequiredItems(Player player)
    {
        var canUsePortal = true;
        if (ItemConditionOp == 1)
        {
            foreach (var itemId in RequiredItems)
            {
                var itemFromId = ItemCatalog.GetItemFromId(itemId);

                if (itemFromId == null)
                    break;

                if (itemFromId.SubCategoryId == ItemSubCategory.TrailPass && player.UserInfo.Member)
                    break;
                
                if (!player.Character.Inventory.Items.ContainsKey(itemId))
                {
                    canUsePortal = false;
                    break;
                }
            }
        }
        else if (ItemConditionOp == 2)
        {
            canUsePortal = false;
            foreach (var itemId in RequiredItems)
            {
                var itemFromId = ItemCatalog.GetItemFromId(itemId);

                if (itemFromId == null)
                    continue;

                if (itemFromId.SubCategoryId == ItemSubCategory.TrailPass && player.UserInfo.Member)
                {
                    canUsePortal = true;
                    break;
                }
                
                if (player.Character.Inventory.Items.ContainsKey(itemId))
                {
                    canUsePortal = true;
                    break;
                }
            }
        }
        return canUsePortal;
    }

    private bool CheckRequiredQuests(Player player)
    {
        var canUsePortal = true;
        if (QuestConditionOp == 1)
        {
            foreach (var quest in RequiredQuests)
            {
                var questFromId = QuestCatalog.GetQuestData(quest);

                if (questFromId == null)
                    break;
                
                var questLineData = QuestCatalog.GetQuestLineData(questFromId.QuestLineId);
                
                if (questLineData != null && !questLineData.ShowInJournal)
                    break;
                
                if (!player.Character.CompletedQuests.Contains(quest))
                {
                    canUsePortal = false;
                    break;
                }
            }
        }
        else if (QuestConditionOp == 2)
        {
            canUsePortal = false;
            foreach (var quest in RequiredQuests)
            {
                var questFromId = QuestCatalog.GetQuestData(quest);

                if (questFromId == null)
                    continue;
                
                var questLineData = QuestCatalog.GetQuestLineData(questFromId.QuestLineId);
                
                if (questLineData != null && !questLineData.ShowInJournal)
                    continue;
                
                if (player.Character.CompletedQuests.Contains(quest))
                {
                    canUsePortal = true;
                    break;
                }
            }
        }
        return canUsePortal;
    }

    public bool CheckRequiredLevels(Player player) => player.Character.GlobalLevel >= RequiredLevel;
}
