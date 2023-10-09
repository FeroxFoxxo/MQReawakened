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
using static LeaderBoardTopScoresJson;
using static NPCController;

namespace Server.Reawakened.Entities;

public class NpcControllerEntity : SyncedEntity<NPCController>
{
    public FollowCamModes CameraMode => EntityData.CameraMode;
    public FollowCamPriority CameraPriority => EntityData.CameraPriority;
    public bool ShouldDisableNpcInteraction => EntityData.ShouldDisableNPCInteraction;

    public ILogger<NpcControllerEntity> Logger { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public InternalVendorCatalog VendorCatalog { get; set; }
    public MiscTextDictionary MiscText { get; set; }
    public ServerRConfig RConfig { get; set; }
    public Dialog Dialog { get; set; }

    public VendorInfo VendorInfo;

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
                                SendNpcInfo(player.Character, player.NetState);
                                Logger.LogDebug("[INTERACTION] [AVALIABLE QUEST] {Name} ({Id})", Name, Id);
                                break;
                            case NPCStatus.QuestInProgress:
                                SendQuestProgress(player.Character, player.NetState);
                                Logger.LogDebug("[INTERACTION] [IN PROGRESS QUEST] Interaction from {Name} ({Id})", Name, Id);
                                break;
                            case NPCStatus.QuestCompleted:
                                ValidateQuest(player.Character, player.NetState);
                                Logger.LogDebug("[INTERACTION] [COMPLETED QUEST] Interaction from {Name} ({Id})", Name, Id);
                                break;
                            case NPCStatus.QuestUnavailable:
                                Logger.LogDebug("[INTERACTION] [DIALOG] Interaction from {Name} ({Id}) [UNIMPLEMENTED]", Name, Id);
                                break;
                            default:
                                break;
                        }

                        SendNpcInfo(player.Character, player.NetState);
                        break;
                    default:
                        Logger.LogWarning("[INTERACTION] [UNKNOWN NPC] Interaction from {Name} ({Id})", PrefabName, Id);
                        break;
                }
            }
        }
        else
        {
            Logger.LogWarning("[INTERACTION] [UNKNOWN EVENT] Interaction of {Type} from {Name} ({Id})", syncEvent.Type, PrefabName, Id);
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
            default:
                break;
        }

        netState.SendXt("nt", Id, (int)npcStatus, descriptionId);
    }

    public NPCStatus GetQuestStatus(CharacterModel character)
    {
        var validatorCount = 0;
        var questStatus = NPCStatus.QuestAvailable;

        foreach (var validatorQuest in ValidatorQuests)
            if (character.Data.CompletedQuests.Contains(validatorQuest.Id))
                validatorCount++;

        if (validatorCount >= ValidatorQuests.Length)
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
                else
                {
                    questStatus = NPCStatus.QuestInProgress;
                    Logger.LogTrace("[{QuestId}] [IN PROGRESS QUEST] Quest from {NpcName} ({Id}) has been started",
                        questState.Id, NpcName, Id);
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

        foreach (var requiredQuest in quest.PreviousQuests)
        {
            if (!character.Data.CompletedQuests.Contains(requiredQuest.Key))
            {
                Logger.LogTrace("[{QuestName} ({QuestId})] [INCOMPLETE REQUIRED QUEST] {ReqQuestId} is required",
                    quest.Name, quest.Id, requiredQuest.Key);
                return false;
            }
        }

        return true;
    }

    public void ValidateQuest(CharacterModel character, NetState netState)
    {
        foreach (var quest in ValidatorQuests)
        {
            var matchingQuest = character.Data.QuestLog.FirstOrDefault(q => q.Id == quest.Id);

            if (matchingQuest == null || character.Data.CompletedQuests.Contains(quest.Id))
                continue;

            if (matchingQuest.QuestStatus != QuestState.TO_BE_VALIDATED)
                continue;

            netState.SendXt("nl", matchingQuest, Id, NameId, Dialog.QuestDialog[quest.Name][2]);

            break;
        }
    }

    private void SendQuestProgress(CharacterModel character, NetState netState)
    {
        foreach (var quest in GiverQuests)
        {
            var matchingQuest = character.Data.QuestLog.FirstOrDefault(q => q.Id == quest.Id);

            if (matchingQuest == null || character.Data.CompletedQuests.Contains(quest.Id))
                continue;

            netState.SendXt("nl", matchingQuest, Id, NameId, Dialog.QuestDialog[quest.Name][1]);

            break;
        }
    }

    public void StartNewQuest(Player player)
    {
        foreach (var givenQuest in GiverQuests)
        {
            if (CanStartQuest(player.Character, givenQuest))
            {
                player.AddQuest(givenQuest, true);

                Logger.LogTrace("[{QuestName} ({QuestId})] [ADD QUEST] Added by {Name}", givenQuest.Name, givenQuest.Id, NpcName);

                if (player.Character.TryGetQuest(givenQuest.Id, out var quest))
                {
                    player.NetState.SendXt("nl", quest, Id, NameId, Dialog.QuestDialog[givenQuest.Name][0]);

                    quest.QuestStatus = QuestState.IN_PROCESSING;
                }
                return;
            }
        }
    }
}
