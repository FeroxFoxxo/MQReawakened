using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Protocols.External._d__DescriptionHandler;
public class RequestPortalInfo : ExternalProtocol
{
    public override string ProtocolName => "dp";

    public WorldGraph WorldGraph { get; set; }
    public MiscTextDictionary MiscText { get; set; }
    public InternalPortalInfos PortalInfos { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ILogger<RequestPortalInfo> Logger { get; set; }

    public override void Run(string[] message)
    {
        var portalId = int.Parse(message[5]);
        var levelId = int.Parse(message[6]);

        var newLevelId = WorldGraph.GetLevelFromPortal(levelId, portalId);
        var newLevelName = WorldGraph.GetInfoLevel(newLevelId).InGameName;
        var newLevelNameId = MiscText.LocalizationDict.FirstOrDefault(x => x.Value == newLevelName);

        var collectedIdols = Player.Character.CollectedIdols.TryGetValue(newLevelId, out var value) ? value.Count : 0;

        if (string.IsNullOrEmpty(newLevelName))
        {
            Logger.LogError("Could not find level for portal {portal} in room {room}", portalId, levelId);
            return;
        }

        var portalInfos = PortalInfos.GetPortalInfos(levelId, portalId);

        var isLockedOrPremium = 0;

        var portalConditions = new SeparatedStringBuilder('#');

        if (portalInfos != null && portalInfos.PortalConditions.Count > 0)
        {
            if (!portalInfos.CheckConditions(Player))
                isLockedOrPremium = 1;
            if (portalInfos.ShowPremiumPortal)
                isLockedOrPremium = 2;
            if (!portalInfos.CheckConditions(Player) && portalInfos.ShowPremiumPortal)
                isLockedOrPremium = 3;

            foreach (var condition in portalInfos.PortalConditions)
            {
                portalConditions.Append(condition.RequiredItems.Count); // Item Id count

                if (condition.RequiredItems.Count > 0)
                    foreach (var item in condition.RequiredItems)
                    {
                        var itemDescription = QuestCatalog.ItemCatalog.GetItemFromId(item);

                        if (itemDescription != null)
                            portalConditions.Append(itemDescription.ItemId);
                    }

                portalConditions.Append(condition.RequiredItems.Count); // Item Name Id count

                if (condition.RequiredItems.Count > 0)
                    foreach (var item in condition.RequiredItems)
                    {
                        var itemDescription = QuestCatalog.ItemCatalog.GetItemFromId(item);

                        if (itemDescription != null)
                        {
                            var itemName = QuestCatalog.ItemCatalog.GetTextIdFromName(itemDescription.ItemName);

                            portalConditions.Append(itemName);
                        }
                    }

                portalConditions.Append(condition.CheckRequiredItems(Player) ? 1 : 0);

                portalConditions.Append(condition.RequiredQuests.Count); // Quest Id count

                if (condition.RequiredQuests.Count > 0)
                    foreach (var quest in condition.RequiredQuests)
                    {
                        var questData = QuestCatalog.GetQuestData(quest);

                        if (questData != null)
                            portalConditions.Append(questData.Id);
                    }

                portalConditions.Append(condition.RequiredQuests.Count); // Quest Name Id count

                if (condition.RequiredQuests.Count > 0)
                    foreach (var quest in condition.RequiredQuests)
                    {
                        var questData = QuestCatalog.GetQuestData(quest);

                        if (questData != null)
                        {
                            var questNameId = MiscText.LocalizationDict.FirstOrDefault(x => x.Value == questData.Title).Key;

                            portalConditions.Append(questNameId);
                        }
                    }

                var requiredLevel = condition.RequiredLevels.Count > 0 ? condition.RequiredLevels.FirstOrDefault().Value : 1;

                portalConditions.Append(condition.CheckRequiredQuests(Player) ? 1 : 0);
                portalConditions.Append(requiredLevel);
                portalConditions.Append(condition.CheckRequiredLevels(Player) ? 1 : 0);
                portalConditions.Append(0); // Subscription Type id
                portalConditions.Append(0); // Is Subscription required
                portalConditions.Append(condition.RequiredQuests.Count > 1 ? 1 : 0); // QuestConditionOp
                portalConditions.Append(condition.RequiredItems.Count > 1 ? 1 : 0); // QuestConditionOp Item
            }
        }

        SendXt("dp", portalId, levelId, isLockedOrPremium, collectedIdols, newLevelNameId.Key,
                portalInfos != null ? portalInfos.PortalConditions.Count : 0, portalInfos != null ? portalConditions.ToString() : 0);
    }
}
