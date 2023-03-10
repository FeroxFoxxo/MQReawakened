using A2m.Server;
using FollowCamDefines;
using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Models;

namespace Server.Reawakened.Entities;

public class NpcControllerEntity : SyncedEntity<NPCController>
{
    private bool _vendorOpen;

    public NpcDescription Description;
    public string NpcName;
    public QuestDescription[] Quests;
    public FollowCamModes CameraMode => EntityData.CameraMode;
    public FollowCamPriority CameraPriority => EntityData.CameraPriority;
    public bool ShouldDisableNpcInteraction => EntityData.ShouldDisableNPCInteraction;

    public ILogger<NpcControllerEntity> Logger { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public NpcCatalog NpcCatalog { get; set; }
    public MiscTextDictionary MiscText { get; set; }

    public override void InitializeEntity()
    {
        Description = NpcCatalog.GetNpc(Id);

        Quests = QuestCatalog.GetQuestsBy(Id).OrderBy(x => x.Id).ToArray();

        if (Description == null) return;

        NpcName = MiscText.GetLocalizationTextById(Description.NameTextId);
    }

    public override object[] GetInitData(Player player) =>
        Description == null ? Array.Empty<object>() : new object[] { Description.NameTextId.ToString() };

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (Description == null)
        {
            Logger.LogWarning("No description found for NPC! Id: {id}", Id);
            return;
        }

        var character = player.Character;

        if (Description.Status is > NPCController.NPCStatus.Dialog and < NPCController.NPCStatus.Unknown)
        {
            _vendorOpen = !_vendorOpen;
            if (_vendorOpen)
            {
                player.SendXt("nv", Id, Description.NameTextId, 0, 0, 0, Description.VendorId, "", "1021|2813",
                    "1021|2812");
                return;
            }
        }

        if (Quests == null) return;

        var status = TryGetQuest(character, Quests, out var model);

        if (status == NPCController.NPCStatus.QuestAvailable)
        {
            player.SendXt("nl", $"{model}&", Id, NpcName ?? "", "1516|3843");
            character.Data.QuestLog.Add(model);
            player.SendXt("nt", Id, (int)NPCController.NPCStatus.QuestInProgress, 0);
        }
        else if (status != NPCController.NPCStatus.Unknown)
        {
            player.SendXt("nt", Id, (int)status, 0);
        }
    }

    public void SendNpcInfo(CharacterModel character, NetState netState)
    {
        if (Description == null) return;

        if (Quests == null)
        {
            if (Description.Status != NPCController.NPCStatus.Unknown)
                netState.SendXt("nt", Id, (int)Description.Status, 0);

            return;
        }

        var status = TryGetQuest(character, Quests, out _);

        if (status == NPCController.NPCStatus.Unknown)
        {
            if (Description.Status != NPCController.NPCStatus.Unknown)
                netState.SendXt("nt", Id, (int)Description.Status, 0);
        }
        else
        {
            netState.SendXt("nt", Id, (int)status, 0);
            Logger.LogTrace("Npc: {n} - {s}", NpcName, status);
        }
    }

    private NPCController.NPCStatus TryGetQuest(CharacterModel character, IEnumerable<QuestDescription> quests,
        out QuestStatusModel outQuest)
    {
        outQuest = null;

        foreach (var quest in quests.Where(quest => !character.Data.CompletedQuests.Contains(quest.Id)))
        {
            if (!character.HasDiscoveredTribe(quest.Tribe))
                return NPCController.NPCStatus.QuestUnavailable;

            if (!character.HasPreviousQuests(quest))
            {
                outQuest = GetQuestStatusModel(quest);
                return NPCController.NPCStatus.QuestUnavailable;
            }

            if (quest.LevelRequired > character.Data.GlobalLevel)
            {
                outQuest = GetQuestStatusModel(quest);
                return NPCController.NPCStatus.QuestUnavailable;
            }

            if (character.HasQuest(quest.Id))
            {
                outQuest = GetQuestStatusModel(quest);
                return NPCController.NPCStatus.QuestInProgress;
            }

            var qld = QuestCatalog.GetQuestLineData(quest.QuestLineId);
            if (qld == null)
            {
                outQuest = GetQuestStatusModel(quest);
                return NPCController.NPCStatus.QuestAvailable;
            }

            var lineQuests = QuestCatalog.GetQuestLineQuests(qld);
            if (lineQuests == null)
            {
                outQuest = GetQuestStatusModel(quest);
                return NPCController.NPCStatus.QuestAvailable;
            }

            foreach (var lineQuest in lineQuests.Where(lineQuest =>
                         !character.Data.CompletedQuests.Contains(lineQuest.Id)))
            {
                if (!character.HasDiscoveredTribe(lineQuest.Tribe))
                    return NPCController.NPCStatus.QuestUnavailable;

                if (!character.HasPreviousQuests(lineQuest))
                {
                    outQuest = GetQuestStatusModel(lineQuest);
                    return NPCController.NPCStatus.QuestUnavailable;
                }

                if (lineQuest.LevelRequired > character.Data.GlobalLevel)
                {
                    outQuest = GetQuestStatusModel(lineQuest);
                    return NPCController.NPCStatus.QuestUnavailable;
                }

                if (character.HasQuest(lineQuest.Id))
                {
                    outQuest = GetQuestStatusModel(lineQuest);
                    return NPCController.NPCStatus.QuestInProgress;
                }

                outQuest = GetQuestStatusModel(lineQuest);
                return NPCController.NPCStatus.QuestAvailable;
            }
        }

        return NPCController.NPCStatus.Unknown;
    }

    private static QuestStatusModel GetQuestStatusModel(QuestDescription quest)
    {
        var model = new QuestStatusModel
        {
            Id = quest.Id,
            QuestStatus = QuestStatus.QuestState.NOT_START
        };

        foreach (var obj in quest.Objectives)
        {
            model.Objectives.Add(obj.Key, new ObjectiveModel
            {
                Completed = false,
                CountLeft = 0
            });
        }

        return model;
    }
}
