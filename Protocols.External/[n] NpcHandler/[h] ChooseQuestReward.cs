using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using static A2m.Server.QuestStatus;
using System.Text;

namespace Protocols.External._n__NpcHandler;

public class ChooseQuestReward : ExternalProtocol
{
    public override string ProtocolName => "nh";

    public ILogger<ChooseQuestReward> Logger { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var npcId = int.Parse(message[5]);
        var questId = int.Parse(message[6]);
        var itemId = int.Parse(message[7]);
        var questRewardId = int.Parse(message[8]);

        if (itemId > 0)
        {
            var item = ItemCatalog.GetItemFromId(itemId);

            if (item != null)
                Player.AddItem(item, 1);
            else
                Logger.LogError("[Quest Validator {NpcId}] Unknown item reward with id: {RewardId}", npcId, itemId);
        }

        if (questRewardId > 0) 
        {
            var newQuest = QuestCatalog.QuestCatalogs[questRewardId];

            if (newQuest != null)
            {
                var oQuest = Player.AddQuest(newQuest, true);

                var rewardIds = (Dictionary<int, int>)newQuest.GetField("_rewardItemsIds");
                var unknownRewards = rewardIds.Where(x => !ItemCatalog.Items.ContainsKey(x.Key));

                if (unknownRewards.Any())
                {
                    var sb = new StringBuilder();

                    foreach (var reward in unknownRewards)
                        sb.AppendLine($"Reward Id {reward.Key}, Count {reward.Value}");
                }

                Player.UpdateNpcsInLevel(newQuest);

                Logger.LogInformation("[{QuestName} ({QuestId})] [QUEST STARTED]", newQuest.Name, newQuest.Id);
            }
        }

        var quest = QuestCatalog.QuestCatalogs[questId];

        Player.AddBananas(quest.BananaReward);
        Player.AddReputation(quest.ReputationReward);

        foreach (var item in quest.RewardItems)
            Player.AddItem(item.Key, item.Value);

        Player.SendUpdatedInventory(false);
    }
}
