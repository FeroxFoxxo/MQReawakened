using FollowCamDefines;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Bundles.Models;
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

    public VendorInfo VendorInfo;
    public QuestDescription[] Quests;

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

        Quests = [.. QuestCatalog.GetQuestsBy(Id).OrderBy(x => x.Id)];

        if (Quests.Length > 0)
        {
            NpcType = NpcType.Quest;
            NameId = (from item in Quests.Select(x => (int)x.GetField("_questGiverNameId"))
                      group item by item into g
                      orderby g.Count() descending
                      select g.Key).First();
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

            switch (NpcType)
            {
                case NpcType.Vendor:
                    if (tEvent.Activate)
                        player.SendXt("nv", VendorInfo.ToString(player));
                    break;
                case NpcType.Quest:
                    break;
                default:
                    Logger.LogWarning("Unknown NPC {Name} ({Id})", PrefabName, Id);
                    break;
            }
        }
        else
        {
            Logger.LogWarning("Unknown Sync Event {Type} for NPC {Name} ({Id})", syncEvent.Type, PrefabName, Id);
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
                npcStatus = NPCStatus.QuestUnavailable;
                break;
            default:
                break;
        }

        netState.SendXt("nt", Id, (int)npcStatus, descriptionId);
    }
}
