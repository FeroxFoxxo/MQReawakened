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

    private readonly NetStateHandler _handler = handler;

    private readonly InternalRwConfig _internalWConfig = internalWConfig;
    private readonly ILogger<PacketHandler> _logger = logger;

    private readonly Dictionary<string, SystemCallback> _protocolsSystem = new();
    private readonly Dictionary<string, ExternalCallback> _protocolsXt = new();

    private readonly ReflectionUtils _reflectionUtils = reflectionUtils;
    private readonly ServerRConfig _serverConfig = serverConfig;
    private readonly IServiceProvider _services = services;
    private readonly EventSink _sink = sink;

    public void Initialize()
    {
        _sink.ServerStarted += AddProtocols;
        _sink.WorldLoad += AskProtocolIgnore;
    }

    private void AskProtocolIgnore()
    {
        if (_internalWConfig.IgnoreProtocolType.Length >= _serverConfig.DefaultProtocolTypeIgnore.Length)
            return;

        if (!_logger.Ask(
                $"It's recommended to add the protocols '{string.Join(", ", _serverConfig.DefaultProtocolTypeIgnore)}' " +
                "to the server ignore config, as to reduce spam. Please press 'y' to enable this. " +
                "You are able to add to this later in the related config file.", true))
            return;

        var internalDebugs = _internalWConfig.IgnoreProtocolType.ToList();

        foreach (var protocol in _serverConfig.DefaultProtocolTypeIgnore)
        {
            if (!internalDebugs.Contains(protocol))
                internalDebugs.Add(protocol);
        }

        _internalWConfig.IgnoreProtocolType = [.. internalDebugs];
    }

    private void AddProtocols(ServerStartedEventArgs e)
    {
        foreach (var type in e.Modules.Select(m => m.GetType().Assembly.GetTypes())
                     .SelectMany(sl => sl).Where(myType => myType.IsClass && !myType.IsAbstract))
        {
            if (type.IsSubclassOf(typeof(SystemProtocol)))
            {
                var createInstance =
                    _reflectionUtils.CreateBuilder<SystemProtocol>(type.GetTypeInfo());

                void Callback(NetState state, XmlDocument document, IServiceProvider services)
                {
                    var instance = createInstance(services);

                    instance.InitializeProtocol(state);
                    instance.Run(document);
                }

                _protocolsSystem.Add(createInstance(_services).ProtocolName, Callback);
            }
            else if (type.IsSubclassOf(typeof(ExternalProtocol)))
            {
                var createInstance =
                    _reflectionUtils.CreateBuilder<ExternalProtocol>(type.GetTypeInfo());

                void Callback(NetState state, string[] msg, IServiceProvider services)
                {
                    var instance = createInstance(services);

                    instance.InitializeProtocol(state);
                    instance.Run(msg);
                }

                _protocolsXt.Add(createInstance(_services).ProtocolName, Callback);
            }
        }

        _handler.Protocols.Add('%', SendXt);
        _handler.Protocols.Add('<', SendSys);
    }

    public ProtocolResponse SendXt(NetState netState, string packet)
    {
        var splitPacket = packet.Split('%');
        var actionType = splitPacket[3];
        var unhandled = false;

        if (_protocolsXt.TryGetValue(actionType, out var value))
        {
            value(netState, splitPacket, _services);
        }
        else
        {
            netState.TracePacketError(actionType, packet);
            unhandled = true;
        }

        return new ProtocolResponse(actionType, unhandled);
    }

    public ProtocolResponse SendSys(NetState netState, string packet)
    {
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(packet);
        var actionType = xmlDocument.SelectSingleNode("/msg/body/@action")?.Value;
        var unhandled = false;

        if (actionType != null && _protocolsSystem.TryGetValue(actionType, out var value))
        {
            value(netState, xmlDocument, _services);
        }
        else
        {
            netState.TracePacketError(actionType, packet);
            unhandled = true;
        }

        return new ProtocolResponse(actionType, unhandled);
    }
}
