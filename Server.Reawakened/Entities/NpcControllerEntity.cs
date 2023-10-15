using A2m.Server;
using FollowCamDefines;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Models;
using static A2m.Server.QuestStatus;
using static NPCController;

namespace Server.Reawakened.Entities;

public class NpcControllerEntity : SyncedEntity<NPCController>
{
    public FollowCamModes CameraMode => EntityData.CameraMode;
    public FollowCamPriority CameraPriority => EntityData.CameraPriority;
    public bool ShouldDisableNpcInteraction => EntityData.ShouldDisableNPCInteraction;

    public ILogger<NpcControllerEntity> Logger { get; set; }
    public MiscTextDictionary MiscText { get; set; }
    public ServerRConfig RConfig { get; set; }
    public Dialog Dialog { get; set; }

    public QuestCatalog QuestCatalog { get; set; }
    public InternalVendorCatalog VendorCatalog { get; set; }
    public InternalDialogCatalog DialogCatalog { get; set; }

    public VendorInfo VendorInfo;
    public DialogInfo DialogInfo;

    public QuestDescription[] ValidatorQuests;
    public QuestDescription[] GiverQuests;

    public int NameId;
    public string NpcName;

    public NpcType NpcType;

    public override void InitializeEntity()
    {
        NameId = -1;
        NpcType = NpcType.Unknown;

        VendorInfo = VendorCatalog.GetVendorById(Id);

        if (VendorInfo != null)
        {
            NpcType = NpcType.Vendor;
            NameId = VendorInfo.NameId;
        }

        GiverQuests = [.. QuestCatalog.GetQuestGiverById(Id).OrderBy(x => x.Id)];
        ValidatorQuests = [.. QuestCatalog.GetQuestValidatorById(Id).OrderBy(x => x.Id)];

        var questGiverNames = GiverQuests.Select(x => (int)x.GetField("_questGiverNameId"));
        var questValidatorNames = GiverQuests.Select(x => (int)x.GetField("_validatorNameId"));
        var allQuestNames = questGiverNames.Concat(questValidatorNames).Where(i => i > 0).ToArray();

        if (allQuestNames.Length > 0)
        {
            NpcType = NpcType.Quest;
            NameId = (from item in allQuestNames
                      group item by item into g
                      orderby g.Count() descending
                      select g.Key)
                      .First();
        }

        DialogInfo = DialogCatalog.GetDialogById(Id);

        if (DialogInfo != null && NpcType == NpcType.Unknown)
        {
            NpcType = NpcType.Dialog;
            NameId = DialogInfo.NameId;
        }

        if (NameId < 0)
        {
            Logger.LogWarning("No information found for NPC {Name} ({Id})", PrefabName, Id);
            return;
        }

        NpcName = MiscText.GetLocalizationTextById(NameId);
    }

    public override object[] GetInitData(Player player) => NameId <= 0 ? [] : [NameId.ToString()];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (syncEvent.Type == SyncEvent.EventType.Trigger)
        {
            var tEvent = new Trigger_SyncEvent(syncEvent);

            if (tEvent.Activate)
            {
                switch (NpcType)
                {
                    case NpcType.Vendor:
                        player.SendXt("nv", VendorInfo.ToString(player));
                        break;
                    case NpcType.Quest:
                        switch (GetQuestStatus(player.Character))
                        {
                            case NPCStatus.QuestAvailable:
                                StartNewQuest(player);
                                Logger.LogDebug("[AVALIABLE QUEST] [{Name} ({Id})]", Name, Id);
                                break;
                            case NPCStatus.QuestInProgress:
                                SendQuestProgress(player);
                                Logger.LogDebug("[IN PROGRESS QUEST] [{Name} ({Id})]", Name, Id);
                                break;
                            case NPCStatus.QuestCompleted:
                                ValidateQuest(player);
                                Logger.LogDebug("[COMPLETED QUEST] [{Name} ({Id})]", Name, Id);
                                break;
                            case NPCStatus.QuestUnavailable:
                                SendDialog(player);
                                Logger.LogDebug("[DIALOG QUEST] [{Name} ({Id})]", Name, Id);
                                break;
                            default:
                                break;
                        }
                        break;
                    case NpcType.Dialog:
                        SendDialog(player);
                        Logger.LogDebug("[DIALOG] [{Name} ({Id})]", Name, Id);
                        break;
                    default:
                        Logger.LogDebug("[UNKNOWN NPC INTERACTION] [{Name} ({Id})]", Name, Id);
                        break;
                }
            }
            else
            {
                Logger.LogDebug("[INACTIVE NPC TRIGGERED] [{Name} ({Id})]", Name, Id);
            }
        }
        else
        {
            Logger.LogDebug("[UNKNOWN NPC EVENT] [{Type}] [{Name} ({Id})]", syncEvent.Type.ToString().ToUpperInvariant(), Name, Id);
        }
    }

    public void SendNpcInfo(CharacterModel character, NetState netState)
    {
        var npcStatus = NPCStatus.Unknown;
        var descriptionId = 0;

        switch (NpcType)
        {
            case NpcType.Vendor:
                npcStatus = VendorInfo.VendorType;
                descriptionId = VendorInfo.DescriptionId;
                break;
            case NpcType.Quest:
                npcStatus = GetQuestStatus(character);
                break;
            case NpcType.Dialog:
                npcStatus = NPCStatus.Dialog;
                descriptionId = DialogInfo.DescriptionId;
                break;
            default:
                break;
        }

        netState.SendXt("nt", Id, (int)npcStatus, descriptionId);
    }

    public void SendDialog(Player player)
    {
        if (DialogInfo == null)
        {
            Logger.LogError("[DIALOG] [{NpcName} ({Id})] No dialog catalog found for NPC",
                NpcName, Id);
            return;
        }

        var dialog = DialogInfo.Dialog
            .Where(d => d.Key <= player.Character.Data.GlobalLevel)
            .OrderBy(d => d.Key)
            .Select(d => d.Value)
            .FirstOrDefault();

        if (dialog == null)
        {
            Logger.LogError("[DIALOG] [{NpcName} ({Id})] No dialog found for user of level {Level}",
                NpcName, Id, player.Character.Data.GlobalLevel);
            return;
        }

        player.NetState.SendXt("nd", Id, NameId, dialog);
    }

    public NPCStatus GetQuestStatus(CharacterModel character)
    {
        var giverCount = 0;
        var questStatus = NPCStatus.QuestAvailable;

        foreach (var giverQuest in GiverQuests)
            if (character.Data.CompletedQuests.Contains(giverQuest.Id))
                giverCount++;

        if (giverCount >= GiverQuests.Length)
        {
            questStatus = NPCStatus.Dialog;
            Logger.LogTrace("[DIALOG] [{NpcName} ({Id})] All quests have been completed", NpcName, Id);
        }

        foreach (var givenQuest in GiverQuests)
        {
            if (CanStartQuest(character, givenQuest))
            {
                questStatus = NPCStatus.QuestAvailable;
                Logger.LogTrace("[{Quest} ({QuestId})] [AVAILABLE QUEST] Availale from {NpcName} ({Id})",
                    givenQuest.Name, givenQuest.Id, NpcName, Id);
                break;
            }
            else
            {
                questStatus = NPCStatus.QuestUnavailable;
                Logger.LogTrace("[{Quest} ({QuestId})] [UNAVAILABLE QUEST] Cannot start quest from {NpcName} ({Id})",
                    givenQuest.Name, givenQuest.Id, NpcName, Id);
            }
        }

        foreach (var questState in character.Data.QuestLog)
        {
            var catalogedQuest = QuestCatalog.GetQuestData(questState.Id);

            if (ValidatorQuests.Any(v => v.QuestLineId == catalogedQuest.QuestLineId && v.Id == questState.Id))
            {
                if (questState.QuestStatus == QuestState.TO_BE_VALIDATED)
                {
                    questStatus = NPCStatus.QuestCompleted;
                    Logger.LogTrace("[{QuestId}] [COMPLETED QUEST] Quest from {NpcName} ({Id}) has been validated",
                        questState.Id, NpcName, Id);
                    break;
                }
            }

            if (GiverQuests.Any(v => v.QuestLineId == catalogedQuest.QuestLineId && v.Id == questState.Id))
            {
                if (questState.QuestStatus == QuestState.IN_PROCESSING)
                {
                    questStatus = NPCStatus.QuestInProgress;
                    Logger.LogTrace("[{QuestId}] [QUEST IN PROGRESS] Quest from {NpcName} ({Id}) is in progress",
                        questState.Id, NpcName, Id);
                    break;
                }
            }
        }

        return questStatus;
    }

    public bool CanStartQuest(CharacterModel character, QuestDescription quest)
    {
        if (character.Data.CompletedQuests.Contains(quest.Id))
        {
            Logger.LogTrace("[{QuestName} ({QuestId})] [ALREADY COMPLETED]", quest.Name, quest.Id);
            return false;
        }

        if (character.Data.QuestLog.Any(q => q.Id == quest.Id))
        {
            Logger.LogTrace("[{QuestName} ({QuestId})] [ALREADY IN PROGRESS]", quest.Name, quest.Id);
            return false;
        }

        if (!QuestCatalog.QuestLineCatalogs.TryGetValue(quest.QuestLineId, out var questLine))
        {
            Logger.LogTrace("[{QuestName} ({QuestId})] [INVALID QUESTLINE] Quest with line {QuestLineId} could not be found",
                quest.Name, quest.Id, quest.QuestLineId);
            return false;
        }

        if (questLine.QuestType == QuestType.Daily || !questLine.ShowInJournal || QuestCatalog.GetQuestLineTotalQuestCount(questLine) == 0)
        {
            Logger.LogTrace("[{QuestName} ({QuestId})] [SKIPPED QUESTLINE] Quest with line {QuestLineId} was skipped as it does not meet valid preconditions",
                quest.Name, quest.Id, quest.QuestLineId);
            return false;
        }

        if (character.Data.GlobalLevel < quest.LevelRequired)
        {
            Logger.LogTrace("[{QuestName} ({QuestId})] [SKIPPED QUEST] Level {Current} is less than required {Required}",
                quest.Name, quest.Id, character.Data.GlobalLevel, quest.LevelRequired);
            return false;
        }

        foreach (var requiredQuest in QuestCatalog.GetListOfPreviousQuests(quest))
            if (character.Data.CompletedQuests.Contains(requiredQuest.Id))
                return true;

        Logger.LogTrace("[{QuestName} ({QuestId})] [DOES NOT MEET REQUIRED QUEST]", quest.Name, quest.Id);

        return false;
    }

    public void ValidateQuest(Player player)
    {
        foreach (var quest in ValidatorQuests)
        {
            var matchingQuest = player.Character.Data.QuestLog.FirstOrDefault(q => q.Id == quest.Id);

            if (matchingQuest == null || player.Character.Data.CompletedQuests.Contains(quest.Id))
                continue;

            if (matchingQuest.QuestStatus != QuestState.TO_BE_VALIDATED)
                continue;

            var questName = quest.Name;

            if (quest.ValidatorGoId != quest.QuestGiverGoId)
                questName += "validator";

            player.NetState.SendXt("nl", matchingQuest, Id, NameId, Dialog.QuestDialog[questName][1]);

            var completedQuest = player.Character.Data.QuestLog.FirstOrDefault(x => x.Id == quest.Id);

            if (completedQuest != null)
            {
                player.Character.Data.QuestLog.Remove(completedQuest);
                player.NetState.SendXt("nq", completedQuest.Id);
                player.Character.Data.CompletedQuests.Add(completedQuest.Id);
                player.UpdateNpcsInLevel();
            }

            break;
        }
    }

    private void SendQuestProgress(Player player)
    {
        foreach (var quest in GiverQuests)
        {
            var matchingQuest = player.Character.Data.QuestLog.FirstOrDefault(q => q.Id == quest.Id);

            if (matchingQuest == null || player.Character.Data.CompletedQuests.Contains(quest.Id))
                continue;

            player.NetState.SendXt("nl", matchingQuest, Id, NameId, Dialog.QuestDialog[quest.Name][2]);

            break;
        }
    }

    public void StartNewQuest(Player player)
    {
        foreach (var givenQuest in GiverQuests)
        {
            if (CanStartQuest(player.Character, givenQuest))
            {
                player.AddQuest(givenQuest, givenQuest.Id, true);

                Logger.LogTrace("[{QuestName} ({QuestId})] [ADD QUEST] Added by {Name}", givenQuest.Name, givenQuest.Id, NpcName);

                if (player.Character.TryGetQuest(givenQuest.Id, out var quest))
                {
                    player.NetState.SendXt("nl", quest, Id, NameId, Dialog.QuestDialog[givenQuest.Name][0]);

                    quest.QuestStatus = QuestState.IN_PROCESSING;
                }

                player.UpdateNpcsInLevel(givenQuest);

                return;
            }
        }
    }
}
