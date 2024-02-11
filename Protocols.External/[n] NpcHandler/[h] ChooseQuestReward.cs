using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using static A2m.Server.QuestStatus;
using System.Text;
using Server.Base.Logging;
using Server.Reawakened.Network.Extensions;

namespace Protocols.External._n__NpcHandler;

public class ChooseQuestReward : ExternalProtocol
{
    public override string ProtocolName => "nh";

    public ILogger<ChooseQuestReward> Logger { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public FileLogger FileLogger { get; set; }

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
            var newQuest = QuestCatalog.GetQuestData(questRewardId);

            if (newQuest != null)
                Player.AddQuest(newQuest, Logger, ItemCatalog, FileLogger, $"Quest reward from {npcId}");

            Player.UpdateAllNpcsInLevel();
        }

        var quest = QuestCatalog.QuestCatalogs[questId];

        Player.AddBananas(quest.BananaReward);
        Player.AddReputation(quest.ReputationReward);

        foreach (var item in quest.RewardItems)
            Player.AddItem(item.Key, item.Value);

        Player.SendUpdatedInventory(false);
    }
}
