using A2m.Server;
using A2m.Server.Protocol;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Reawakened.Players;
using Server.Reawakened.Thrift.Abstractions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Thrift.Protocol;
using static A2m.Server.Protocol.DescriptionHandlerServer;

namespace Server.Reawakened.Thrift.Protocols;

public class DescriptionHandler(ILogger<DescriptionHandler> logger, WorldGraph worldGraph,
    MiscTextDictionary miscTextDictionary, QuestCatalog questCatalog, InternalPortalInfos portalInfos) : ThriftHandler(logger)
{
    public override void AddProcesses(Dictionary<string, ProcessFunction> processes) =>
        processes.Add("getPortalInfo", GetPortalInfo);

    private void GetPortalInfo(ThriftProtocol protocol, NetState netState)
    {
        var args = new getPortalInfo_args();
        args.Read(protocol.InProtocol);

        protocol.InProtocol.ReadMessageEnd();

        var portalId = int.Parse(args.GoId);

        var newLevelId = worldGraph.GetLevelFromPortal(args.LevelId, portalId);
        var newLevelName = worldGraph.GetInfoLevel(newLevelId).InGameName;
        var newLevelNameId = miscTextDictionary.LocalizationDict.FirstOrDefault(x => x.Value == newLevelName);

        var player = netState.Get<Player>();

        var collectedIdols = player.Character.CollectedIdols;

        var portalInfo = portalInfos.GetPortalInfos(newLevelId, portalId);

        var portalConditions = new List<A2m.Server.Protocol.PortalCondition>();

        if (portalInfo != null && portalInfo.PortalConditions.Count > 0)
        {
            var requiredQuestNames = new List<int>();
            var requiredItemNames = new List<int>();

            foreach (var condition in portalInfo.PortalConditions)
            {
                if (condition.RequiredQuests.Count > 0)
                {
                    foreach (var quest in condition.RequiredQuests)
                    {
                        var questData = questCatalog.GetQuestData(quest);
                        var questNameId = miscTextDictionary.LocalizationDict.FirstOrDefault(x => x.Value == questData.Title).Key;

                        if (!requiredQuestNames.Contains(questNameId))
                            requiredQuestNames.Add(questNameId);
                    }
                }

                if (condition.RequiredItems.Count > 0)
                {
                    foreach (var item in condition.RequiredItems)
                    {
                        var itemDescription = questCatalog.ItemCatalog.GetItemFromId(item);
                        var itemName = questCatalog.ItemCatalog.GetTextIdFromName(itemDescription.ItemName);

                        if (!requiredItemNames.Contains(itemName))
                            requiredItemNames.Add(itemName);
                    }
                }

                var requiredLevel = condition.RequiredLevels.Count > 0 ? condition.RequiredLevels.FirstOrDefault().Value : 1;

                portalConditions.Add(new A2m.Server.Protocol.PortalCondition()
                {
                    ItemIds = condition.RequiredItems,
                    ItemNameIds = requiredItemNames,
                    QuestIds = condition.RequiredQuests,
                    QuestNameIds = requiredQuestNames,
                    HasFinishedQuests = condition.CheckRequiredQuests(player),
                    OwnItems = condition.CheckRequiredItems(player),
                    ReputationLevel = requiredLevel,
                    HasReputation = condition.CheckRequiredLevels(player),
                    OperatorItem = 0,
                    OperatorQuest = 0
                });
            }
        }

        var result = new getPortalInfo_result
        {
            Success = new PortalInfo
            {
                LevelId = args.LevelId,
                IdolCount = collectedIdols.TryGetValue(newLevelId, out var value) ? value.Count : 0,
                GoId = portalId,
                IsLocked = portalInfo != null && !portalInfo.CheckConditions(player),
                IsMemberOnly = false,
                IsPremium = portalInfo != null && portalInfo.ShowPremiumPortal,
                DestinationLevelNameId = newLevelNameId.Key,
                Conditions = portalInfo != null ? portalConditions : []
            }
        };

        protocol.OutProtocol.WriteMessageBegin(
            new TMessage("getPortalInfo", TMessageType.Reply, protocol.SequenceId)
        );

        result.Write(protocol.OutProtocol);

        protocol.OutProtocol.WriteMessageEnd();
        protocol.OutProtocol.Transport.Flush();
    }
}
