using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.GameObjects.NPC;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;

namespace Protocols.External._n__NpcHandler;

public class ChooseQuestReward : ExternalProtocol
{
    public override string ProtocolName => "nh";

    public ILogger<ChooseQuestReward> Logger { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public FileLogger FileLogger { get; set; }
    public InternalQuestItem QuestItems { get; set; }
    public ServerRConfig Config { get; set; }
    public InternalAchievement InternalAchievement { get; set; }

    public override void Run(string[] message)
    {
        var npcId = int.Parse(message[5]);
        var questId = int.Parse(message[6]);
        var itemId = int.Parse(message[7]);
        var questRewardId = int.Parse(message[8]);

        Player.SendXt("nq", questId);

        if (itemId > 0)
        {
            var item = ItemCatalog.GetItemFromId(itemId);

            if (item != null)
                Player.AddItem(item, 1, ItemCatalog);
            else
                Logger.LogError("[Quest Validator {NpcId}] Unknown item reward with id: {RewardId}", npcId, itemId);
        }

        if (questRewardId > 0)
        {
            var newQuest = QuestCatalog.GetQuestData(questRewardId);

            if (newQuest != null)
                Player.AddQuest(newQuest, QuestItems, ItemCatalog, FileLogger, $"Quest reward from {npcId}", Logger);

            Player.UpdateAllNpcsInLevel();
        }

        var quest = QuestCatalog.QuestCatalogs[questId];
        var questLine = QuestCatalog.GetQuestLineData(quest.QuestLineId);

        if (questLine.QuestType == QuestType.Main)
        {
            var questGiver = Player.Room.GetEntityFromId<NPCControllerComp>(npcId.ToString());
            questGiver.StartNewQuest(Player);
        }

        Player.CheckAchievement(AchConditionType.CompleteQuest, [quest.Name], InternalAchievement, Logger); // Specific Quest by name for example EVT_SB_1_01
        Player.CheckAchievement(AchConditionType.CompleteQuestInLevel, [Player.Room.LevelInfo.Name], InternalAchievement, Logger); // Quest by Level/Trail if any exist

        Player.AddBananas(quest.BananaReward, InternalAchievement, Logger);
        Player.AddReputation(quest.ReputationReward, Config);

        foreach (var item in quest.RewardItems)
            Player.AddItem(item.Key, item.Value, ItemCatalog);

        Player.SendUpdatedInventory();
    }
}
