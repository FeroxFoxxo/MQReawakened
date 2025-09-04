﻿using A2m.Server;
using FollowCamDefines;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Entities.Components.GameObjects.NPC.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.Misc;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;
using Server.Reawakened.XMLs.Data.Npcs;
using static A2m.Server.QuestStatus;
using static NPCController;

namespace Server.Reawakened.Entities.Components.GameObjects.NPC;

public class NPCControllerComp : Component<NPCController>
{
    public FollowCamModes CameraMode => ComponentData.CameraMode;
    public FollowCamPriority CameraPriority => ComponentData.CameraPriority;
    public bool ShouldDisableNpcInteraction => ComponentData.ShouldDisableNPCInteraction;

    public ILogger<NPCControllerComp> Logger { get; set; }
    public FileLogger FileLogger { get; set; }

    public QuestCatalog QuestCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public DialogDictionary Dialog { get; set; }
    public MiscTextDictionary MiscText { get; set; }

    public ServerRConfig Config { get; set; }

    public InternalAchievement InternalAchievement { get; set; }
    public InternalVendor VendorCatalog { get; set; }
    public InternalDialog DialogCatalog { get; set; }
    public InternalDialogRewrite DialogRewrites { get; set; }
    public InternalQuestItem QuestItems { get; set; }

    public VendorInfo VendorInfo;
    public DialogInfo DialogInfo;

    public QuestDescription[] ValidatorQuests;
    public QuestDescription[] GiverQuests;

    public int NameId;
    public string NpcName;

    public NpcType NpcType;

    public override void InitializeComponent()
    {
        NameId = -1;
        NpcType = NpcType.Unknown;

        if (Config.GameVersion <= GameVersion.vLate2013)
            GiverQuests = [];

        VendorInfo = VendorCatalog.GetVendorById(Room.LevelInfo.LevelId, int.Parse(Id));

        if (VendorInfo != null)
        {
            NpcType = NpcType.Vendor;
            NameId = VendorInfo.NameId;
        }

        DialogInfo = DialogCatalog.GetDialogById(Room.LevelInfo.LevelId, int.Parse(Id));

        if (DialogInfo != null)
        {
            NpcType = NpcType.Dialog;
            NameId = DialogInfo.NameId;
        }

        ValidatorQuests = [..
            QuestCatalog.GetQuestValidatorById(int.Parse(Id))
                .Where(x => x.ValidatorLevelId == Room.LevelInfo.LevelId)
        ];

        GiverQuests = Config.GameVersion >= GameVersion.vEarly2014
            ? [..
                QuestCatalog.GetQuestGiverById(int.Parse(Id))
                .Where(x => x.QuestGiverLevelId == Room.LevelInfo.LevelId)
            ] : [..
                QuestCatalog.GetQuestGiverByName(
                    MiscText.GetLocalizationTextById(NameId > 0 ? NameId : GetNameId())
                )
            ];

        GiverQuests = [.. GiverQuests.OrderBy(x => x.Id)];
        ValidatorQuests = [.. ValidatorQuests.OrderBy(x => x.Id)];

        var questName = GetNameId();

        if (questName > 0)
        {
            NpcType = NpcType.Quest;
            NameId = questName;
        }

        if (NameId < 0)
        {
            Logger.LogWarning("No information found for NPC {Name} ({Id})", PrefabName, Id);
            return;
        }

        NpcName = MiscText.GetLocalizationTextById(NameId);
    }

    public int GetNameId()
    {
        var questGiverNames = GiverQuests.Select(x => (int)x.GetField("_questGiverNameId"));
        var questValidatorNames = ValidatorQuests.Select(x => (int)x.GetField("_validatorNameId"));
        var allQuestNames = questGiverNames.Concat(questValidatorNames).Where(i => i > 0).ToArray();

        return (from item in allQuestNames
                group item by item into g
                orderby g.Count() descending
                select g.Key).FirstOrDefault();
    }

    public override object[] GetInitData(Player player) => NameId <= 0 ? [] : [NameId.ToString()];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (syncEvent.Type == SyncEvent.EventType.Trigger)
        {
            var tEvent = new Trigger_SyncEvent(syncEvent);

            if (tEvent.Activate)
                TalkToNpc(player);
            else
                Logger.LogDebug("[INACTIVE NPC TRIGGERED] [{Name} ({Id})]", NpcName, Id);
        }
        else
            Logger.LogDebug("[UNKNOWN NPC EVENT] [{Type}] [{Name} ({Id})]", syncEvent.Type.ToString().ToUpperInvariant(), NpcName, Id);

        SendNpcInfo(player);
    }

    public void TalkToNpc(Player player)
    {
        RunObjectives(player);

        switch (NpcType)
        {
            case NpcType.Vendor:
                player.SendXt("nv", VendorInfo.ToString(player));
                break;
            case NpcType.Quest:
                switch (GetQuestStatus(player))
                {
                    case NPCStatus.QuestAvailable:
                        StartNewQuest(player);
                        Logger.LogDebug("[AVALIABLE QUEST] [{Name} ({Id})]", NpcName, Id);
                        break;
                    case NPCStatus.QuestInProgress:
                        SendQuestProgress(player);
                        Logger.LogDebug("[IN PROGRESS QUEST] [{Name} ({Id})]", NpcName, Id);
                        break;
                    case NPCStatus.QuestCompleted:
                        ValidateQuest(player);
                        Logger.LogDebug("[COMPLETED QUEST] [{Name} ({Id})]", NpcName, Id);
                        break;
                    case NPCStatus.QuestUnavailable:
                        SendDialog(player);
                        Logger.LogDebug("[DIALOG QUEST] [{Name} ({Id})]", NpcName, Id);
                        break;
                    case NPCStatus.Dialog:
                        SendDialog(player);
                        Logger.LogDebug("[DIALOG QUEST] [{Name} ({Id})]", NpcName, Id);
                        break;
                    default:
                        break;
                }
                break;
            case NpcType.Dialog:
                SendDialog(player);
                Logger.LogDebug("[DIALOG] [{Name} ({Id})]", NpcName, Id);
                break;
            default:
                Logger.LogDebug("[UNKNOWN NPC INTERACTION] [{Name} ({Id})]", NpcName, Id);
                SendUnknownDialogChat(player);
                break;
        }

        player.CheckAchievement(AchConditionType.Talkto, [PrefabName], InternalAchievement, Logger);
    }

    public void RunObjectives(Player player)
    {
        player.CheckObjective(ObjectiveEnum.Talkto, Id, PrefabName, 1, QuestCatalog);
        player.CheckObjective(ObjectiveEnum.Goto, Id, PrefabName, 1, QuestCatalog);
        player.CheckObjective(ObjectiveEnum.HiddenGoto, Id, PrefabName, 1, QuestCatalog);
    }

    public void SendNpcInfo(Player player)
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
                npcStatus = GetQuestStatus(player);
                break;
            case NpcType.Dialog:
                npcStatus = NPCStatus.Dialog;
                descriptionId = DialogInfo.DescriptionId;
                break;
            default:
                break;
        }

        player.SendXt("nt", Id, (int)npcStatus, descriptionId);
    }

    public void SendDialog(Player player)
    {
        if (DialogInfo == null)
            if (!string.IsNullOrEmpty(NpcName))
            {
                var foundDialog = Dialog.GenericDialog.FirstOrDefault(x => x.Key.Contains(NpcName + Id) && x.Value.Count > 0).Value;

                if (foundDialog == null)
                {
                    Logger.LogError("[DIALOG] [{NpcName} ({Id})] No dialog catalog found for NPC", NpcName, Id);
                    return;
                }

                var foundConversation = foundDialog.FirstOrDefault();

                var dialogEntry = new Dictionary<int, ConversationInfo>
                {
                    {
                        1,
                        new ConversationInfo(foundConversation.DialogId, foundConversation.ConversationId)
                    }
                };

                DialogInfo = new DialogInfo(int.Parse(Id), NameId, 0, dialogEntry);
            }
            else
                return;

        var dialog = DialogInfo.Dialog
            .Where(d => d.Key <= player.Character.GlobalLevel)
            .OrderBy(d => d.Key)
            .Select(d => d.Value)
            .FirstOrDefault();

        if (dialog == null)
        {
            Logger.LogError("[DIALOG] [{NpcName} ({Id})] No dialog found for user of level {Level}",
                NpcName, Id, player.Character.GlobalLevel);
            SendUnknownDialogChat(player);
            return;
        }

        player.NetState.SendXt("nd", Id, NameId, dialog);
    }

    public void SendUnknownDialogChat(Player player) =>
        player.Chat(CannedChatChannel.Speak, "SERVER", $"NPC Name: '{NpcName}' Prefab: '{PrefabName}' ({Id})\n" +
            "Whoops! Looks like you've interacted with an NPC that doesn't have any dialog. " +
            $"Please message a developer about this, including the above information.");

    public NPCStatus GetQuestStatus(Player player)
    {
        Logger.LogDebug("Looping through VALIDATOR quests for {NpcName} ({Id})", NpcName, Id);
        foreach (var validatorQuest in ValidatorQuests)
        {
            var questStatus = GetQuestType(player, validatorQuest.Id);

            if (questStatus == NPCStatus.Dialog)
            {
                Logger.LogTrace("[QUESTLINE COMPLETED] Questline from {NpcName} ({Id}) is completed", NpcName, Id);
                return NPCStatus.Dialog;
            }

            if (questStatus == NPCStatus.QuestCompleted)
            {
                Logger.LogTrace("[{QuestId}] [COMPLETED QUEST] Quest from {NpcName} ({Id}) has been validated",
                    validatorQuest.Id, NpcName, Id);

                return NPCStatus.QuestCompleted;
            }
        }

        Logger.LogDebug("Looping through GIVER quests for {NpcName} ({Id})", NpcName, Id);

        foreach (var givenQuest in GiverQuests)
        {
            var questStatus = GetQuestType(player, givenQuest.Id);

            if (questStatus == NPCStatus.Dialog)
            {
                Logger.LogTrace("[QUESTLINE COMPLETED] Questline from {NpcName} ({Id}) is completed", NpcName, Id);
                return NPCStatus.Dialog;
            }

            if (questStatus == NPCStatus.QuestInProgress)
            {
                Logger.LogTrace("[{QuestId}] [QUEST IN PROGRESS] Quest from {NpcName} ({Id}) is in progress",
                    givenQuest.Id, NpcName, Id);

                return NPCStatus.QuestInProgress;
            }

            if (questStatus == NPCStatus.QuestAvailable)
            {
                Logger.LogTrace("[{Quest} ({QuestId})] [AVAILABLE QUEST] Available from {NpcName} ({Id})",
                    givenQuest.Name, givenQuest.Id, NpcName, Id);

                return NPCStatus.QuestAvailable;
            }
        }

        Logger.LogDebug("Finished looping quests for {NpcName} ({Id}), found none", NpcName, Id);

        return NPCStatus.QuestUnavailable;
    }

    private NPCStatus GetQuestType(Player player, int questId)
    {
        var questData = QuestCatalog.GetQuestData(questId);

        if (QuestCatalog.QuestLineCatalogs.TryGetValue(questData.QuestLineId, out var questLine))
        {
            var giverQuest = GiverQuests.OrderBy(x => x.QuestgGiverName == NpcName && questLine.ShowInJournal);
            var validatorQuest = ValidatorQuests.OrderBy(x => x.ValidatorName == NpcName && questLine.ShowInJournal);

            if (giverQuest.All(y => player.Character.CompletedQuests.Any(z => y.Id == z))
                && validatorQuest.All(y => player.Character.CompletedQuests.Any(z => y.Id == z))
                && !giverQuest.Any(x => player.Character.QuestLog.Any(y => x.Id == y.Id))
                && !validatorQuest.Any(x => player.Character.QuestLog.Any(y => x.Id == y.Id))
                && questLine.QuestType != QuestType.Daily)
            {
                Logger.LogTrace("[{QuestLineId}] [COMPLETED QUESTLINE] Completed {NpcName}'s questline.", questData.QuestLineId, NpcName);
                return VendorInfo != null ? VendorInfo.VendorType : NPCStatus.Dialog;
            }
        }

        if (player.Character.CompletedQuests.Contains(questId))
        {
            Logger.LogTrace("[{QuestName} ({QuestId})] [ALREADY COMPLETED]", questData.Name, questData.Id);
            return NPCStatus.Unknown;
        }

        var currentQuest = player.Character.QuestLog.FirstOrDefault(q => q.Id == questId);

        if (currentQuest != null)
        {
            if (currentQuest.QuestStatus == QuestState.IN_PROCESSING)
            {
                var incompleteQuestObjs = currentQuest.Objectives.Values.Where(o => !o.Completed);
                var incompleteObjectives = incompleteQuestObjs.Count();

                var minOrder = 0;

                if (incompleteObjectives > 0)
                    minOrder = incompleteQuestObjs.Min(x => x.Order);

                foreach (var incompleteQuestObj in incompleteQuestObjs.Where(x => x.Order == minOrder))
                {
                    if (incompleteQuestObj.ObjectiveType == ObjectiveEnum.Deliver)
                        if (player.Character.Inventory.Items.TryGetValue(incompleteQuestObj.ItemId, out var item))
                            if (item.Count >= incompleteQuestObj.CountLeft)
                                incompleteObjectives--;

                    if (incompleteQuestObj.GameObjectId.ToString() == Id &&
                        incompleteQuestObj.GameObjectLevelId == Room.LevelInfo.LevelId &&
                        incompleteQuestObj.ObjectiveType is ObjectiveEnum.Talkto or ObjectiveEnum.Goto or ObjectiveEnum.HiddenGoto)
                    {
                        incompleteQuestObj.Completed = true;
                        incompleteQuestObj.CountLeft = 0;
                        return GetQuestType(player, questId);
                    }
                }

                if (incompleteObjectives <= 0)
                    currentQuest.QuestStatus = QuestState.TO_BE_VALIDATED;
            }

            if (currentQuest.QuestStatus == QuestState.IN_PROCESSING)
            {
                Logger.LogTrace("[{QuestName} ({QuestId})] [ALREADY IN PROGRESS]", questData.Name, questData.Id);
                return NPCStatus.QuestInProgress;
            }
            else if (currentQuest.QuestStatus == QuestState.TO_BE_VALIDATED)
            {
                Logger.LogTrace("[{QuestName} ({QuestId})] [TO BE VALIDATED]", questData.Name, questData.Id);
                return questData.ValidatorGoId.ToString() == Id ? NPCStatus.QuestCompleted : NPCStatus.QuestInProgress;
            }
        }

        if (!QuestCatalog.QuestLineCatalogs.TryGetValue(questData.QuestLineId, out questLine))
        {
            Logger.LogTrace("[{QuestName} ({QuestId})] [INVALID QUESTLINE] Quest with line {QuestLineId} could not be found",
                questData.Name, questData.Id, questData.QuestLineId);
            return NPCStatus.Unknown;
        }

        if (!questLine.ShowInJournal || QuestCatalog.GetQuestLineTotalQuestCount(questLine) == 0)
        {
            Logger.LogTrace("[{QuestName} ({QuestId})] [SKIPPED QUESTLINE] Quest with line {QuestLineId} was skipped as it does not meet valid preconditions",
                questData.Name, questData.Id, questData.QuestLineId);
            return NPCStatus.Unknown;
        }

        if (player.Character.GlobalLevel < questData.LevelRequired)
        {
            Logger.LogTrace("[{QuestName} ({QuestId})] [SKIPPED QUEST] Level {Current} is less than required {Required}",
                questData.Name, questData.Id, player.Character.GlobalLevel, questData.LevelRequired);
            return NPCStatus.Unknown;
        }

        if (questLine.QuestType == QuestType.Daily)
            if (!CanStartDailyQuest(player, questData.Id.ToString()))
            {
                Logger.LogTrace("[{QuestName}] ({QuestId}) [SKIPPED QUEST] Daily quest has been completed already.", questData.Name, questData.Id);
                return NPCStatus.Unknown;
            }

        if (Config.GameVersion < GameVersion.vEarly2014 && questData.Name == "T4IR_00_01" 
            && !player.Character.CompletedQuests.Contains(939))
        {
            Logger.LogTrace("[{QuestName}] ({QuestId}) [SKIPPED QUEST] Not all tribe tutorial quests are completed.", questData.Name, questData.Id);
            return NPCStatus.Unknown;
        }

        var requiredQuests = QuestCatalog.GetAllQuestLineRequiredQuest(questLine);
        var previousQuests = QuestCatalog.GetListOfPreviousQuests(questData);

        var canStartQuest = false;

        if (Config.GameVersion >= GameVersion.vEarly2014)
        {
            canStartQuest = previousQuests.Count == 0;

            foreach (var previousQuest in previousQuests)
                if (player.Character.CompletedQuests.Contains(previousQuest.Id))
                {
                    canStartQuest = true;
                    break;
                }
        }
        else
        {
            canStartQuest = requiredQuests.Count == 0;

            foreach (var requiredQuest in requiredQuests)
                if (player.Character.CompletedQuests.Contains(requiredQuest.Id))
                {
                    foreach (var previousQuest in previousQuests)
                        if (player.Character.CompletedQuests.Contains(previousQuest.Id))
                        {
                            canStartQuest = true;
                            break;
                        }
                    break;
                }
        }

        if (canStartQuest)
        {
            Logger.LogDebug("[{QuestName} ({QuestId})] [FOUND QUEST THAT MEETS REQUIREMENTS]", questData.Name, questData.Id);
            return NPCStatus.QuestAvailable;
        }
        else
        {
            if (Config.GameVersion < GameVersion.vEarly2014)
                previousQuests = [.. previousQuests, .. requiredQuests];

            Logger.LogTrace(
                "[{QuestName} ({QuestId})] [DOES NOT MEET REQUIRED QUESTS] Previous Quests: {PrevQuests}",
                questData.Name, questData.Id,
                string.Join(", ", previousQuests.Select(x => $"{x.Name} ({x.Id})"))
            );
        }

        return NPCStatus.Unknown;
    }

    public void ValidateQuest(Player player)
    {
        foreach (var quest in ValidatorQuests)
        {
            var matchingQuest = player.Character.QuestLog.FirstOrDefault(q => q.Id == quest.Id);

            if (matchingQuest == null || player.Character.CompletedQuests.Contains(quest.Id) && QuestCatalog.GetQuestLineData(quest.QuestLineId).QuestType != QuestType.Daily)
                continue;

            if (matchingQuest.QuestStatus != QuestState.TO_BE_VALIDATED)
                continue;

            SendNpcDialog(player, matchingQuest, QuestState.TO_BE_VALIDATED);

            var completedQuest = player.Character.QuestLog.FirstOrDefault(x => x.Id == quest.Id);

            if (completedQuest != null)
            {
                foreach (var trigger in Room.GetEntitiesFromType<IQuestTriggered>())
                    trigger.QuestCompleted(quest, player);

                Logger.LogInformation("[{QuestName} ({QuestId})] [QUEST COMPLETED]", quest.Name, quest.Id);
            }

            break;
        }
    }

    private void SendQuestProgress(Player player)
    {
        var quests = GiverQuests.Concat(ValidatorQuests);

        foreach (var quest in quests)
        {
            var matchingQuest = player.Character.QuestLog.FirstOrDefault(q => q.Id == quest.Id);

            if (matchingQuest == null || player.Character.CompletedQuests.Contains(quest.Id))
                continue;

            SendNpcDialog(player, matchingQuest, QuestState.IN_PROCESSING);

            break;
        }
    }

    public void StartNewQuest(Player player)
    {
        foreach (var givenQuest in GiverQuests)
            if (GetQuestType(player, givenQuest.Id) == NPCStatus.QuestAvailable)
            {
                var isDaily = QuestCatalog.GetQuestLineData(givenQuest.QuestLineId).QuestType == QuestType.Daily;
                var quest = player.AddQuest(givenQuest, QuestItems, ItemCatalog, FileLogger, NpcName, Logger, true, isDaily);

                SendNpcDialog(player, quest, QuestState.NOT_START);

                return;
            }
    }

    public void SendNpcDialog(Player player, QuestStatusModel questStatus, QuestState state)
    {
        var quest = QuestCatalog.GetQuestData(questStatus.Id);
        var questName = quest.Name;

        if ((quest.ValidatorName != quest.QuestgGiverName || quest.ValidatorGoId != quest.QuestGiverGoId && quest.QuestGiverGoId > 0) && quest.ValidatorGoId.ToString() == Id)
            questName += "validator";

        if (DialogRewrites.Rewrites[Config.GameVersion].TryGetValue(questName, out var rewrittenName))
            questName = rewrittenName;

        if (!Dialog.QuestDialog.TryGetValue(questName, out var questDialog))
        {
            Logger.LogError("[{QuestName}] [UNKNOWN QUEST DIALOG]", questName);
            return;
        }

        var dialogNumber = state switch
        {
            QuestState.IN_PROCESSING => 2,
            QuestState.NOT_START => 0,
            QuestState.TO_BE_VALIDATED => 1,
            _ => -1,
        };

        if (questDialog.Count < dialogNumber)
        {
            Logger.LogError("[{QuestName}] [UNKNOWN DIALOG ID {DialogId}]", questName, dialogNumber);
            return;
        }

        var dialog = questDialog[dialogNumber];

        if (!Dialog.DialogDict.TryGetValue(dialog.DialogId, out var dialogXml))
        {
            Logger.LogError("[{QuestName}] [UNKNOWN XML DIALOG ID {DialogId}]", questName, dialog.DialogId);
            return;
        }

        var conversation = dialogXml.FirstOrDefault(x => x.ConversationId == dialog.ConversationId);

        if (conversation == null)
        {
            Logger.LogError("[{QuestName}] [UNKNOWN XML CONVERSATION ID {DialogId}]", questName, dialog.ConversationId);
            return;
        }

        if (conversation.Lines.All(x => x.TextId == 0) || conversation.Lines.Count == 0)
        {
            Logger.LogError("[{QuestName}] [UNKNOWN XML CONVERSATION TEXT {DialogId}]", questName, dialog.ConversationId);
            SendDialog(player);
            return;
        }

        var oQuestStatus = questStatus.DeepCopy();

        oQuestStatus.QuestStatus = state;
        var dialogStr = dialog.ToString();

        player.NetState.SendXt("nl", oQuestStatus, Id, NameId, dialogStr);
    }

    private static bool CanStartDailyQuest(Player player, string dailyObjectId)
    {
        if (!player.Character.CurrentQuestDailies.ContainsKey(dailyObjectId) ||
            player.Character.CurrentQuestDailies.TryGetValue(dailyObjectId, out var dailyObject) &&
            dailyObject.GameObjectId == dailyObjectId && dailyObject.LevelId == player.Room.LevelInfo.LevelId &&
            DateTime.Now.Date > dailyObject.TimeOfHarvest.Date)
        {
            player.Character.CurrentQuestDailies.Remove(dailyObjectId);
            return true;
        }
        else
            return false;
    }
}
