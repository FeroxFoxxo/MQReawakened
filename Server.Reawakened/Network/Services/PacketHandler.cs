using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Events.Arguments;
using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Base.Network.Models;
using Server.Base.Network.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Network.Protocols;
using System.Reflection;
using System.Xml;

namespace Server.Reawakened.Network.Services;

public class PacketHandler(IServiceProvider services, ReflectionUtils reflectionUtils,
    NetStateHandler handler, EventSink sink, ILogger<PacketHandler> logger, ServerRConfig serverConfig,
    InternalRwConfig internalWConfig) : IService
{
    public delegate void ExternalCallback(NetState state, string[] message, IServiceProvider serviceProvider);
    public delegate void SystemCallback(NetState state, XmlDocument document, IServiceProvider serviceProvider);

    private readonly Dictionary<string, SystemCallback> _protocolsSystem = [];
    private readonly Dictionary<string, ExternalCallback> _protocolsExternal = [];

    public void Initialize()
    {
        sink.ServerStarted += AddProtocols;
        sink.WorldLoad += AskProtocolIgnore;
    }

    private void AskProtocolIgnore()
    {
        if (internalWConfig.IgnoreProtocolType.Length >= serverConfig.DefaultProtocolTypeIgnore.Length)
            return;

        if (!logger.Ask(
                $"It's recommended to add the protocols '{string.Join(", ", serverConfig.DefaultProtocolTypeIgnore)}' " +
                "to the server ignore config, as to reduce spam. Please press 'y' to enable this. " +
                "You are able to add to this later in the related config file.", true))
            return;

        var internalDebugs = internalWConfig.IgnoreProtocolType.ToList();

        foreach (var protocol in serverConfig.DefaultProtocolTypeIgnore)
        {
            if (!internalDebugs.Contains(protocol))
                internalDebugs.Add(protocol);
        }

        internalWConfig.IgnoreProtocolType = [.. internalDebugs];
    }

    private void AddProtocols(ServerStartedEventArgs e)
    {
        foreach (var type in e.Modules.Select(m => m.GetType().Assembly.GetTypes())
                     .SelectMany(sl => sl).Where(myType => myType.IsClass && !myType.IsAbstract))
        {
            if (type.IsSubclassOf(typeof(SystemProtocol)))
            {
                var createInstance =
                    reflectionUtils.CreateBuilder<SystemProtocol>(type.GetTypeInfo());

                void Callback(NetState state, XmlDocument document, IServiceProvider services)
                {
                    var instance = createInstance(services);

                    instance.InitializeProtocol(state);
                    instance.Run(document);
                }

                _protocolsSystem.Add(createInstance(services).ProtocolName, Callback);
            }
            else if (type.IsSubclassOf(typeof(ExternalProtocol)))
            {
                var createInstance =
                    reflectionUtils.CreateBuilder<ExternalProtocol>(type.GetTypeInfo());

                void Callback(NetState state, string[] msg, IServiceProvider services)
                {
                    var instance = createInstance(services);

                    instance.InitializeProtocol(state);
                    instance.Run(msg);
                }

                _protocolsExternal.Add(createInstance(services).ProtocolName, Callback);
            }
        }

        handler.ProtocolLookup.Add('%', GetXt);
        handler.ProtocolLookup.Add('<', GetSys);

        handler.ProtocolSend.Add('%', SendXt);
        handler.ProtocolSend.Add('<', SendSys);
    }

    public ProtocolResponse GetXt(string packet)
    {
        var splitPacket = packet.Split('%');
        var actionType = splitPacket[3];
        var unhandled = !_protocolsExternal.ContainsKey(actionType);

        return new ProtocolResponse(actionType, unhandled, splitPacket);
    }

    public ProtocolResponse GetSys(string packet)
    {
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(packet);
        var actionType = xmlDocument.SelectSingleNode("/msg/body/@action")?.Value;
        var unhandled = !_protocolsSystem.ContainsKey(actionType);

        return new ProtocolResponse(actionType, unhandled, xmlDocument);
    }

    public void SendXt(NetState netState, string actionType, object packetStr) =>
        _protocolsExternal[actionType](netState, (string[])packetStr, services);

    public void SendSys(NetState netState, string actionType, object packetXml) =>
        _protocolsSystem[actionType](netState, (XmlDocument)packetXml, services);
}
