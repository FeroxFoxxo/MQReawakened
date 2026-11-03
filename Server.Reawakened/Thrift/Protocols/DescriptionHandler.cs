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
    MiscTextDictionary miscTextDictionary, QuestCatalog questCatalog, UseSubConditionInfos portalInfos) : ThriftHandler(logger)
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

        if (newLevelId <= 0)
            logger.LogError("Could not find level for portal {PortalId} in room {RoomId}", portalId, args.LevelId);
        
        var newLevel = worldGraph.GetInfoLevel(newLevelId);
        var newLevelName = newLevel.InGameName;
        var newLevelNameId = miscTextDictionary.LocalizationDict.FirstOrDefault(x => x.Value == newLevelName);

        var player = netState.Get<Player>();

        var collectedIdols = player.Character.CollectedIdols;

        var portalInfo = portalInfos.GetUseSubConditionInfo(newLevel.UseconditionID);

        var portalConditions = new List<A2m.Server.Protocol.PortalCondition>();

        if (portalInfo != null)
        {
            var requiredQuestNames = new List<int>();
            var requiredItemNames = new List<int>();

            if (portalInfo.QuestsID.Count > 0)
            {
                foreach (var quest in portalInfo.QuestsID)
                {
                    var questData = questCatalog.GetQuestData(quest);

                    if (questData == null)
                        continue;

                    var questNameId = miscTextDictionary.LocalizationDict.FirstOrDefault(x => x.Value == questData.Title).Key;

                    if (!requiredQuestNames.Contains(questNameId))
                        requiredQuestNames.Add(questNameId);
                }
            }

            if (portalInfo.ItemsID.Count > 0)
            {
                foreach (var item in portalInfo.ItemsID)
                {
                    var itemDescription = questCatalog.ItemCatalog.GetItemFromId(item);

                    if (itemDescription == null)
                        continue;

                    var itemName = questCatalog.ItemCatalog.GetTextIdFromName(itemDescription.ItemName);

                    if (!requiredItemNames.Contains(itemName))
                        requiredItemNames.Add(itemName);
                }
            }
            
            portalConditions.Add(new A2m.Server.Protocol.PortalCondition()
            {
                ItemIds = portalInfo.ItemsID,
                ItemNameIds = requiredItemNames,
                QuestIds = portalInfo.QuestsID,
                QuestNameIds = requiredQuestNames,
                HasFinishedQuests = portalInfos.IsSatisfied(player, portalInfo),
                OwnItems = portalInfos.IsSatisfied(player, portalInfo),
                ReputationLevel = portalInfos.GetReputation(portalInfo),
                HasReputation = portalInfos.IsSatisfied(player, portalInfo),
                OperatorItem = portalInfo.ItemOperation,
                OperatorQuest = portalInfo.QuestOperation
            });
        }

        var result = new getPortalInfo_result
        {
            Success = new PortalInfo
            {
                LevelId = args.LevelId,
                IdolCount = collectedIdols.TryGetValue(newLevelId, out var value) ? value.Count : 0,
                GoId = portalId,
                IsLocked = portalInfo != null && !portalInfos.IsSatisfied(player, portalInfo),
                IsMemberOnly = portalInfo != null && portalInfos.IsMemberOnly(portalInfo),
                IsPremium = portalInfo != null && portalInfos.IsPremiumCondition(portalInfo),
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
