using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._d__DescriptionHandler;
public class RequestPortalInfo : ExternalProtocol
{
    public override string ProtocolName => "dp";

    public WorldGraph WorldGraph { get; set; }
    public MiscTextDictionary MiscText { get; set; }
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

        SendXt("dp", portalId, levelId, 0, collectedIdols, newLevelNameId.Key, 0);
    }
}
