using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Base;

public class UseSubConditionInfos : UseSubConditionInfoXML, IBundledXml
{
    public string BundleName => "UseSubConditionInfo";
    public BundlePriority Priority => BundlePriority.Low;

    private Dictionary<int, UseSubConditionInfo> _useSubConditionInfos;
    
    public ItemCatalog ItemCatalog { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    
    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetField<UseSubConditionInfoXML>("_useSubConditionInfoDict", new Dictionary<int, UseSubConditionInfo>());

        _useSubConditionInfos = [];
    }
    
    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle() =>
        _useSubConditionInfos = (Dictionary<int, UseSubConditionInfo>)this.GetField<UseSubConditionInfoXML>("_useSubConditionInfoDict");
    
    public new UseSubConditionInfo GetUseSubConditionInfo(int subConditionID) =>
        _useSubConditionInfos.TryGetValue(subConditionID, out var info) ? info : null;

    public bool IsMemberOnly(UseSubConditionInfo useSubConditionInfo)
    {
        var subscriptionID = int.Parse(useSubConditionInfo.GetField("SubscriptionID").ToString());
        return subscriptionID != 0;
    }

    public bool IsPremiumCondition(UseSubConditionInfo useSubConditionInfo)
    {
        var isPremium = IsMemberOnly(useSubConditionInfo);
        if (!isPremium)
        {
            foreach (var prefabId in useSubConditionInfo.ItemsID)
            {
                var itemFromId = ItemCatalog.GetItemFromId(prefabId);
                if (itemFromId != null && itemFromId.Currency == CurrencyType.NickCash)
                {
                    isPremium = true;
                    break;
                }
            }
        }
        return isPremium;
    }
    
    public bool IsSatisfied(Player player, UseSubConditionInfo useSubConditionInfo)
    {
        var canUsePortal = CheckRequiredLevels(player, useSubConditionInfo);
        
        if (canUsePortal)
            canUsePortal = CheckRequiredItems(player, useSubConditionInfo);
        
        if (canUsePortal)
            canUsePortal = CheckRequiredQuests(player, useSubConditionInfo);
        
        return canUsePortal;
    }

    private bool CheckRequiredLevels(Player player, UseSubConditionInfo useSubConditionInfo)
    {
        var reputation = GetReputation(useSubConditionInfo);
        return player.Character.GlobalLevel >= reputation;
    }

    public int GetReputation(UseSubConditionInfo useSubConditionInfo)
    {
        var reputation = int.Parse(useSubConditionInfo.GetField("Reputation").ToString());
        return reputation;
    }
    
    private bool CheckRequiredItems(Player player, UseSubConditionInfo useSubConditionInfo)
    {
        var canUsePortal = true;
        if (useSubConditionInfo.ItemOperation == 1)
        {
            foreach (var itemId in useSubConditionInfo.ItemsID)
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
        else if (useSubConditionInfo.ItemOperation == 2)
        {
            canUsePortal = false;
            foreach (var itemId in useSubConditionInfo.ItemsID)
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

    private bool CheckRequiredQuests(Player player, UseSubConditionInfo useSubConditionInfo)
    {
        var canUsePortal = true;
        if (useSubConditionInfo.QuestOperation == 1)
        {
            foreach (var quest in useSubConditionInfo.QuestsID)
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
        else if (useSubConditionInfo.QuestOperation == 2)
        {
            canUsePortal = false;
            foreach (var quest in useSubConditionInfo.QuestsID)
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
}
