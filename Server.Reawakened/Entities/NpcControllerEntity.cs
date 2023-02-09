using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Entities;

public class NpcControllerEntity : SyncedEntity<NPCController>
{
    public ILogger<NpcControllerEntity> Logger { get; set; }
    public NPCCatalog NpcCatalog { get; set; }
    public MiscTextDictionary MiscText { get; set; }

    public NpcDescription Description = null;

    public override void InitializeEntity() => Description = NpcCatalog.GetNpcByObjectId(Id);
    
    public override string[] GetInitData(NetState netState)
    {
        if (Description != null)
        {
            var name = MiscText.GetLocalizationTextById(Description.NameTextId);
            Logger.LogDebug("NpcName: {name}", name);
        }
        return Description == null ? Array.Empty<string>() : (new string[] { Description.NameTextId.ToString() });
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        if (Description == null)
        {
            Logger.LogDebug("No Description Found for NPC! Id: {id}", Id);
            return;
        }


    }
}
