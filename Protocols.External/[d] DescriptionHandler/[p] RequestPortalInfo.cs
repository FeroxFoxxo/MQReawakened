using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Protocols.External._d__DescriptionHandler;
public class RequestPortalInfo : ExternalProtocol
{
    public override string ProtocolName => "dp";

    public WorldGraph WorldGraph { get; set; }
    public MiscTextDictionary MiscText { get; set; }
    public InternalPortalInfos PortalInfos { get; set; }
    public ILogger<RequestPortalInfo> Logger { get; set; }

    public override void Run(string[] message)
    {
        var portalId = int.Parse(message[5]);
        var levelId = int.Parse(message[6]);

        var newLevelId = WorldGraph.GetLevelFromPortal(levelId, portalId);
        var newLevelName = WorldGraph.GetInfoLevel(newLevelId).InGameName;
        var newLevelNameId = MiscText.LocalizationDict.FirstOrDefault(x => x.Value == newLevelName);

        var collectedIdols = Player.Character.CollectedIdols.TryGetValue(newLevelId, out var value) ? value.Count : 0;

        if (string.IsNullOrEmpty(newLevelName))
        {
            Logger.LogError("Could not find level for portal {portal} in room {room}", portalId, levelId);
            return;
        }

        var portalInfos = PortalInfos.GetPortalInfos(levelId, portalId);

        var isLocked = 0;
        var isPremium = 0;

        if (portalInfos != null)
        {
            if (!portalInfos.CheckConditions(Player))
                isLocked = 1;
            if (portalInfos.ShowPremiumPortal)
                isPremium = 1;
        }

        SendXt("dp", portalId, levelId, isLocked, collectedIdols, newLevelNameId.Key, isPremium);
    }
}
