using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Events.Arguments;
using Server.Base.Core.Services;
using Server.Base.Logging;
using System.Net;
using System.Net.Sockets;

namespace Server.Base.Network.Services;

public class MessagePump : IService
{
    private readonly IPEndPoint[] _ipEndPoints;
    private readonly ILogger<MessagePump> _logger;
    private readonly FileLogger _fileLogger;
    private readonly ServerHandler _serverHandler;
    private readonly EventSink _sink;
    private readonly IServiceProvider _services;

    public readonly Listener[] Listeners;

    public MessagePump(IServiceProvider services)
    {
        var rwConfig = services.GetRequiredService<InternalRwConfig>();

        _logger = services.GetRequiredService<ILogger<MessagePump>>();
        _fileLogger = services.GetRequiredService<FileLogger>();
        _services = services;
        _sink = services.GetRequiredService<EventSink>();
        _serverHandler = services.GetRequiredService<ServerHandler>();

        _ipEndPoints =
        [
            new(IPAddress.Any, rwConfig.Port)
        ];

        Listeners = new Listener[_ipEndPoints.Length];
    }

    public void Initialize()
    {
        _sink.SocketConnect += SocketConnect;
        _sink.ServerStarted += _ => ServerStarted();
    }

    private void ServerStarted()
    {
        var success = false;

        do
        {
            for (var i = 0; i < _ipEndPoints.Length; i++)
            {
                Listeners[i] = new Listener(_ipEndPoints[i], _fileLogger, _logger, _serverHandler, _sink);
                success = true;
            }

            if (success) continue;

            _logger.LogWarning("Retrying listeners...");

            Thread.Sleep(10000);
        } while (!success);
    }

    private static void SocketConnect(SocketConnectEventArgs @event)
    {
        if (!@event.AllowConnection)
            return;

        @event.Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
    }

    public void Slice()
    {
        foreach (var listener in Listeners)
        {
            var accepted = listener.Slice();

            foreach (var socket in accepted)
            {
                new NetState(socket, _services).Start();
            }
        }
    }
}
