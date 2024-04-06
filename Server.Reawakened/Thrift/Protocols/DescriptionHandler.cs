using A2m.Server.Protocol;
using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Players;
using Server.Reawakened.Thrift.Abstractions;
using Server.Reawakened.XMLs.Bundles.Base;
using Thrift.Protocol;
using static A2m.Server.Protocol.DescriptionHandlerServer;

namespace Server.Reawakened.Thrift.Protocols;

public class DescriptionHandler(ILogger<DescriptionHandler> logger, WorldGraph worldGraph,
    MiscTextDictionary miscTextDictionary) : ThriftHandler(logger)
{
    public override void AddProcesses(Dictionary<string, ProcessFunction> processes) =>
        processes.Add("getPortalInfo", GetPortalInfo);

    private void GetPortalInfo(ThriftProtocol protocol, NetState netState)
    {
        var args = new getPortalInfo_args();
        args.Read(protocol.InProtocol);

        protocol.InProtocol.ReadMessageEnd();

        var portalId = int.Parse(args.GoId);

        var newLevelId = worldGraph.GetLevelFromPortal(args.LevelId, portalId);
        var newLevelName = worldGraph.GetInfoLevel(newLevelId).InGameName;
        var newLevelNameId = miscTextDictionary.LocalizationDict.FirstOrDefault(x => x.Value == newLevelName);

        var player = netState.Get<Player>();

        var collectedIdols = player.Character.CollectedIdols;

        var result = new getPortalInfo_result
        {
            Success = new PortalInfo
            {
                LevelId = args.LevelId,
                IdolCount = collectedIdols.TryGetValue(newLevelId, out var value) ? value.Count : 0,
                GoId = portalId,
                IsLocked = false,
                IsMemberOnly = false,
                IsPremium = false,
                DestinationLevelNameId = newLevelNameId.Key,
                Conditions = []
            }
        };

        protocol.OutProtocol.WriteMessageBegin(
            new TMessage("getPortalInfo", TMessageType.Reply, protocol.SequenceId)
        );

        result.Write(protocol.OutProtocol);

        protocol.OutProtocol.WriteMessageEnd();
        protocol.OutProtocol.Transport.Flush();
    }
}
