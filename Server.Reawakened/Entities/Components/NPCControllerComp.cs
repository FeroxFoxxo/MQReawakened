using A2m.Server;
using FollowCamDefines;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Models.Npcs;
using System.Text;
using static A2m.Server.QuestStatus;
using static NPCController;

namespace Server.Reawakened.Entities.Components;

public class NPCControllerComp : Component<NPCController>
{
    public FollowCamModes CameraMode => ComponentData.CameraMode;
    public FollowCamPriority CameraPriority => ComponentData.CameraPriority;
    public bool ShouldDisableNpcInteraction => ComponentData.ShouldDisableNPCInteraction;

    public ILogger<NPCControllerComp> Logger { get; set; }
    public ServerRConfig RConfig { get; set; }
    public FileLogger FileLogger { get; set; }

    public QuestCatalog QuestCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public DialogDictionary Dialog { get; set; }
    public MiscTextDictionary MiscText { get; set; }

    public ObjectiveCatalogInt ObjectiveCatalog { get; set; }
    public VendorCatalogInt VendorCatalog { get; set; }
    public DialogCatalogInt DialogCatalog { get; set; }
    public DialogRewriteInt DialogRewrites { get; set; }

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
                player.CheckObjective(QuestCatalog, ObjectiveCatalog, ObjectiveEnum.Talkto, Id, "GoToTrigger", 1);

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
                Logger.LogDebug("[INACTIVE NPC TRIGGERED] [{Name} ({Id})]", Name, Id);
        }
        else
            Logger.LogDebug("[UNKNOWN NPC EVENT] [{Type}] [{Name} ({Id})]", syncEvent.Type.ToString().ToUpperInvariant(), Name, Id);
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

        foreach (var quest in character.Data.QuestLog)
        {
            var catalogedQuest = QuestCatalog.GetQuestData(quest.Id);

            if (ValidatorQuests.Any(v => v.QuestLineId == catalogedQuest.QuestLineId && v.Id == quest.Id))
            {
                if (quest.QuestStatus == QuestState.TO_BE_VALIDATED)
                {
                    questStatus = NPCStatus.QuestCompleted;
                    Logger.LogTrace("[{QuestId}] [COMPLETED QUEST] Quest from {NpcName} ({Id}) has been validated",
                        quest.Id, NpcName, Id);
                    break;
                }
                if (quest.QuestStatus == QuestState.IN_PROCESSING)
                {
                    var canSendQuestComplete = true;

                    foreach (var objective in quest.Objectives.Values)
                    {
                        if (objective.Completed)
                            continue;

                        if (objective.CountLeft > 1 ||
                            objective.ObjectiveType != ObjectiveEnum.Talkto ||
                            objective.GameObjectLevelId != Room.LevelInfo.LevelId ||
                            objective.GameObjectId != Id
                        )
                        {
                            canSendQuestComplete = false;
                            break;
                        }
                    }

                    questStatus = canSendQuestComplete ? NPCStatus.QuestCompleted : NPCStatus.QuestInProgress;
                }
            }

            if (GiverQuests.Any(v => v.QuestLineId == catalogedQuest.QuestLineId && v.Id == quest.Id))
                if (quest.QuestStatus == QuestState.IN_PROCESSING)
                {
                    questStatus = NPCStatus.QuestInProgress;
                    Logger.LogTrace("[{QuestId}] [QUEST IN PROGRESS] Quest from {NpcName} ({Id}) is in progress",
                        quest.Id, NpcName, Id);
                    break;
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

            SendNpcDialog(player, matchingQuest, quest, 1);

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
        var quests = GiverQuests.Concat(ValidatorQuests);

        foreach (var quest in quests)
        {
            var matchingQuest = player.Character.Data.QuestLog.FirstOrDefault(q => q.Id == quest.Id);

            if (matchingQuest == null || player.Character.Data.CompletedQuests.Contains(quest.Id))
                continue;

            SendNpcDialog(player, matchingQuest, quest, 2);

            break;
        }
    }

    public void StartNewQuest(Player player)
    {
        foreach (var givenQuest in GiverQuests)
            if (CanStartQuest(player.Character, givenQuest))
            {
                player.AddQuest(givenQuest, givenQuest.Id, true);

                Logger.LogTrace("[{QuestName} ({QuestId})] [ADD QUEST] Added by {Name}", givenQuest.Name, givenQuest.Id, NpcName);

                var rewardIds = (Dictionary<int, int>)givenQuest.GetField("_rewardItemsIds");
                var unknownRewards = rewardIds.Where(x => !ItemCatalog.Items.ContainsKey(x.Key));

                if (unknownRewards.Any())
                {
                    var sb = new StringBuilder();

                    foreach (var reward in unknownRewards)
                        sb.AppendLine($"Reward Id {reward.Key}, Count {reward.Value}");

                    FileLogger.WriteGenericLog<NPCController>("unknown-rewards", $"[Unkwown Quest {givenQuest.Id} Rewards]", sb.ToString(),
                        LoggerType.Error);
                }

                if (player.Character.TryGetQuest(givenQuest.Id, out var quest))
                {
                    SendNpcDialog(player, quest, givenQuest, 0);
                    quest.QuestStatus = QuestState.IN_PROCESSING;
                }

                player.UpdateNpcsInLevel(givenQuest);

                return;
            }
    }

    public void SendNpcDialog(Player player, QuestStatusModel questStatus, QuestDescription quest, int dialogNumber)
    {
        var questName = quest.Name;

        if (quest.ValidatorGoId != quest.QuestGiverGoId && quest.ValidatorGoId == Id)
            questName += "validator";

        if (DialogRewrites.Rewrites.TryGetValue(questName, out var rewrittenName))
            questName = rewrittenName;

        if (!Dialog.QuestDialog.TryGetValue(questName, out var questDialog))
        {
            Logger.LogError("[{QuestName}] [UNKNOWN QUEST]", questName);
            return;
        }

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

        if (!conversation.Lines.Any(x => x.TextId > 0))
            SendDialog(player);

        player.NetState.SendXt("nl", questStatus, Id, NameId, questDialog[dialogNumber]);
    }
}
