using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;
using static LeaderBoardTopScoresJson;

namespace Server.Reawakened.Entities;

public class NpcControllerEntity : SyncedEntity<NPCController>
{
    public ILogger<NpcControllerEntity> Logger { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public NPCCatalog NpcCatalog { get; set; }
    public MiscTextDictionary MiscText { get; set; }

    public NpcDescription Description;
    public List<QuestDescription> Quests;

    public string NpcName;

    public override void InitializeEntity()
    {
        Description = NpcCatalog.GetNpcByObjectId(Id);

        Quests = QuestCatalog.GetQuestsBy(Id);

        if (Description != null)
        {
            NpcName = MiscText.GetLocalizationTextById(Description.NameTextId);
        }
    }

    public override string[] GetInitData(NetState netState) => Description == null ? Array.Empty<string>() : (new string[] { Description.NameTextId.ToString() });

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        if (Description == null)
        {
            Logger.LogDebug("No Description Found for NPC! Id: {id}", Id);
            return;
        }

        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();

        Logger.LogDebug(syncEvent.EncodeData());

        /*
        if (character.Data.QuestLog.Count == 0)
        {
            netState.SendXt("nd", Id, Description.NameTextId, "1516|3843");
            character.Data.QuestLog.Add(GetQuestStatusModel(QuestCatalog.GetQuestData(969)));
        }
        else
        {
            netState.SendXt("nd", Id, Description.NameTextId, "1516|3845");
        }
        */
    }

    public void SendNpcInfo(CharacterModel character, NetState netState)
    {
        if (Description == null) return;

        if (Quests == null)
        {
            if (Description.Status != NPCController.NPCStatus.Unknown) netState.SendXt("nt", Id, (int)Description.Status, 0);
            return;
        }

        QuestStatusModel model = null;
        var status = NPCController.NPCStatus.Unknown;

        foreach(var quest in Quests)
        {
            if (model != null || status != NPCController.NPCStatus.Unknown) break;
            if (character.Data.CompletedQuests.Contains(quest.Id)) continue;
            else if (HasQuest(character, quest.Id)) continue;
            else if (!character.Data.HasDiscoveredTribe(quest.Tribe)) continue;

            var qld = QuestCatalog.GetQuestLineData(quest.QuestLineId);
            if (qld == null)
            {
                if (quest.LevelRequired > character.Data.GlobalLevel) continue;

                status = NPCController.NPCStatus.QuestAvailable;
                model = GetQuestStatusModel(quest);
            }
            else
            {
                var lineQuests = QuestCatalog.GetQuestLineQuests(qld);
                foreach(var lineQuest in lineQuests)
                {
                    if (character.Data.CompletedQuests.Contains(lineQuest.Id)) continue;
                    else if (HasQuest(character, lineQuest.Id))
                    { 
                        status = NPCController.NPCStatus.QuestInProgress;
                        break;
                    } 
                    else if (!character.Data.HasDiscoveredTribe(quest.Tribe))
                    {
                        status = NPCController.NPCStatus.QuestUnavailable;
                        break;
                    }

                    if (lineQuest.LevelRequired > character.Data.GlobalLevel)
                    {
                        status = NPCController.NPCStatus.QuestUnavailable;
                        break;
                    }

                    status = NPCController.NPCStatus.QuestAvailable;
                    model = GetQuestStatusModel(lineQuest);
                    break;
                }
            }
        }

        if (model == null || status == NPCController.NPCStatus.Unknown)
        {
            if (Description.Status != NPCController.NPCStatus.Unknown)
                netState.SendXt("nt", Id, (int)Description.Status, 0);

            netState.SendXt("nl", "", Id, NpcName ?? "", "");
        }
        else
        {
            netState.SendXt("nt", Id, (int)status, 0);
            netState.SendXt("nl", model.ToString(), Id, NpcName ?? "", "");
        }
    }

    private QuestStatusModel GetQuestStatusModel(QuestDescription quest)
    {
        var model = new QuestStatusModel()
        {
            Id = quest.Id,
            QuestStatus = A2m.Server.QuestStatus.QuestState.NOT_START,
        };

        foreach(var obj in quest.Objectives)
        {   
            model.Objectives.Add(obj.Key, new ObjectiveModel()
            {
                Completed = false,
                CountLeft = 0,
            });
        }

        return model;
    }

    public bool HasQuest(CharacterModel character, int questId)
    {
        if (character.Data.QuestLog.Count == 0) return false;

        foreach(var quest in character.Data.QuestLog)
        {
            if (quest.Id == questId) return true;
        }

        return false;
    }

    public bool HasPreviousQuests(CharacterModel character, QuestDescription quest)
    {
        if (character.Data.CompletedQuests.Count == 0) return false;

        foreach (var prevId in quest.PreviousQuests)
        {
            if (prevId.Key == 0) continue;
            if (!character.Data.CompletedQuests.Contains(prevId.Key))
            {
                return false;
            }
        }

        return true;
    }
}
