using Microsoft.Extensions.Logging;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Logging;
using Server.Base.Network.Enums;
using Server.Base.Network.Events;
using Server.Base.Network.Helpers;
using Server.Base.Network.Services;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Base.Network;

public class NetState : IDisposable
{
    public delegate bool ThrottlePacketCallback(NetState state);

    private readonly InternalRConfig _rConfig;
    private readonly InternalRwConfig _rwConfig;

    private readonly Dictionary<Type, INetStateData> _data;

    private readonly NetStateHandler _handler;
    private readonly ILogger<MessagePump> _logger;
    private readonly FileLogger _fileLogger;
    private readonly EventSink _sink;

    public string Identifier;
    public readonly IPAddress Address;
    public readonly object AsyncLock;
    public readonly byte[] Buffer;
    public readonly DateTime ConnectedOn;
    public readonly int UpdateRange;
    private bool _disposing;
    private double _nextCheckActivity;

    private AsyncCallback _onReceiveCallback, _onSendCallback;

    public AsyncStates AsyncState;
    public ThrottlePacketCallback Throttler;

    public Socket Socket { get; private set; }
    public bool Running { get; private set; }

    public NetState(Socket socket, ILogger<MessagePump> logger,
        FileLogger fileLogger, NetStateHandler handler, IpLimiter limiter,
        InternalRwConfig rwConfig, InternalRConfig rConfig, EventSink sink)
    {
        Socket = socket;
        AsyncLock = new object();
        Buffer = new byte[rConfig.BufferSize];

        _logger = logger;
        _fileLogger = fileLogger;
        _handler = handler;
        _rwConfig = rwConfig;
        _rConfig = rConfig;
        _sink = sink;

        _nextCheckActivity = GetTicks.Ticks + _rConfig.DisconnectionTimeout;

        _handler.Instances.Add(this);
        _data = [];

        try
        {
            Address = limiter.Intern(((IPEndPoint)Socket.RemoteEndPoint)?.Address);
            Identifier = Address.ToString();
        }
        catch (Exception ex)
        {
            _handler.TraceNetworkError(ex, this);
            Address = IPAddress.None;
            Identifier = "(error)";
        }

        ConnectedOn = DateTime.UtcNow;

        UpdateRange = rConfig.GlobalUpdateRange;

        _sink.InvokeNetStateAdded(new NetStateAddedEventArgs(this));
    }

    private void WriteServer(string text) =>
        _logger.LogTrace("Outbound {Client}: {Text}", this, text);

    private void WriteClient(string text) =>
        _logger.LogTrace("Inbound  {Client}: {Text}", this, text);

    public void CheckAlive(double curTicks)
    {
        if (Socket == null)
            return;

        if (_nextCheckActivity - curTicks >= 0)
            return;

        _logger.LogError("{NetState}: Disconnecting due to inactivity...", this);

        Dispose();
    }

    public void Start()
    {
        _onReceiveCallback = OnReceive;
        _onSendCallback = OnSend;

        Running = true;

        lock (_handler.Disposed)
        {
            if (Socket == null || _handler.Paused)
                return;

            _logger.LogInformation("{NetState}: Connected. [{Count} Online]", this, _handler.Instances.Count);
        }

        try
        {
            lock (AsyncLock)
            {
                _logger.LogTrace("Beginning receiving information for {NetState}. Using thread culture {Culture}",
                    this, Thread.CurrentThread.CurrentCulture.NativeName);

                if ((AsyncState & (AsyncStates.Pending | AsyncStates.Paused)) == 0)
                    BeginReceive();
            }
        }
        catch (Exception ex)
        {
            lock (_handler.Disposed)
                _handler.TraceNetworkError(ex, this);

            Dispose();
        }
    }

    public void BeginReceive()
    {
        AsyncState |= AsyncStates.Pending;

        if (Socket != null && _onReceiveCallback != null)
            Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, _onReceiveCallback, Socket);
    }

    public void Send(string packet, string protocolType)
    {
        if (Socket == null)
            return;

        if (!string.IsNullOrEmpty(protocolType))
            if (!_rwConfig.IgnoreProtocolType.Contains(protocolType))
                WriteServer(packet);

        packet += "\0";

        var buffer = Encoding.UTF8.GetBytes(packet);
        var length = buffer.Length;

        if (buffer.Length <= 0)
            return;

        if (Socket == null || _onSendCallback == null || _disposing)
            return;

        Socket.BeginSend(buffer, 0, length, SocketFlags.None, _onSendCallback, Socket);
    }

    private void OnSend(IAsyncResult asyncResult)
    {
        var socket = (Socket)asyncResult.AsyncState;

        try
        {
            var bytes = socket?.EndSend(asyncResult);

            if (bytes <= 0)
            {
                Dispose();
                return;
            }

            _nextCheckActivity = GetTicks.Ticks + _rConfig.DisconnectionTimeout;
        }
        catch (Exception)
        {
            Dispose();
        }
    }

    private void OnReceive(IAsyncResult asyncResult)
    {
        var bufferedPacket = "Unknown Packet";

        try
        {
            var socket = (Socket)asyncResult.AsyncState;

            if (socket == null) return;
            var byteCount = socket.EndReceive(asyncResult);

            if (byteCount > 0)
            {
                _nextCheckActivity = GetTicks.Ticks + _rConfig.DisconnectionTimeout;

                lock (AsyncLock)
                {
                    if (Throttler != null)
                        if (!Throttler(this))
                            return;
                        else
                            Throttler = null;
                }

                var buffered = new byte[byteCount];

                lock (AsyncLock)
                    Array.Copy(Buffer, buffered, byteCount);

                bufferedPacket = Encoding.UTF8.GetString(buffered);

                foreach (var packet in bufferedPacket.Split('\0'))
                {
                    if (string.IsNullOrEmpty(packet))
                        continue;

                    NetStateHandler.GetProtocol getProtocolData = null;
                    var protocolType = packet[0];

                    lock (_handler.Disposed)
                    {
                        if (_handler.ProtocolLookup.TryGetValue(protocolType, out var handlerProtocol))
                            getProtocolData = handlerProtocol;
                    }

                    if (getProtocolData != null)
                    {
                        var protocolData = getProtocolData(packet);

                        if (string.IsNullOrEmpty(protocolData.ProtocolId))
                            continue;

                        if (protocolData.IsUnhandled)
                        {
                            AddUnhandledPacket(protocolData.ProtocolId);

                            TracePacketError(protocolData.ProtocolId, packet);
                        }
                        else
                        {
                            RemoveUnhandledPacket(protocolData.ProtocolId);

                            if (!_rwConfig.IgnoreProtocolType.Contains(protocolData.ProtocolId))
                                WriteClient(packet);

                            NetStateHandler.SendProtocol sendProtocolData = null;

                            lock (_handler.Disposed)
                            {
                                if (_handler.ProtocolSend.TryGetValue(protocolType, out var handlerProtocol))
                                    sendProtocolData = handlerProtocol;
                            }

                            sendProtocolData?.Invoke(this, protocolData.ProtocolId, protocolData.PacketData);
                        }
                    }
                    else
                    {
                        TracePacketError(protocolType.ToString(), packet);
                    }
                }

                lock (AsyncLock)
                {
                    AsyncState &= ~AsyncStates.Pending;

                    if ((AsyncState & AsyncStates.Paused) != 0) return;
                    try
                    {
                        BeginReceive();
                    }
                    catch (Exception ex)
                    {
                        _handler.TraceNetworkError(ex, this);
                        Dispose();
                    }
                }
            }
            else
            {
                Dispose();
            }
        }
        catch (Exception ex)
        {
            WriteClient(bufferedPacket);
            lock (_handler.Disposed)
                _handler.TraceNetworkError(ex, this);
            Dispose();
        }
    }

    public override string ToString() => Identifier;

    public T Get<T>() where T : class => !_data.ContainsKey(typeof(T)) ? null : _data[typeof(T)] as T;

    public void Set<T>(T data) where T : INetStateData => _data.Add(typeof(T), data);

    public void RemoveAllData()
    {
        foreach (var data in _data)
        {
            lock (_handler.Disposed)
                data.Value?.RemovedState(this, _handler, _logger);
        }
    }

    public void TraceBufferError(int byteCount)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Unhandled buffered content of size '{byteCount}'");

        _fileLogger.WriteGenericLog<MessagePump>("network-errors", $"Client {this}",
            sb.ToString(), LoggerType.Warning);
    }

    public void TracePacketError(string packetId, string packet)
    {
        if (packet.Length <= 0)
            return;

        var sb = new StringBuilder();

        sb.AppendLine($"Unhandled packet '{packetId}'");
        sb.Append(packet);

        _fileLogger.WriteGenericLog<MessagePump>("network-errors", $"Client {this}",
            sb.ToString(), LoggerType.Warning);
    }

    public void AddUnhandledPacket(string packetId)
    {
        if (_rwConfig.UnhandledPackets.Contains(packetId))
            return;

        var packets = _rwConfig.UnhandledPackets.ToList();
        packets.Add(packetId);
        _rwConfig.UnhandledPackets = [.. packets];
    }

    public void RemoveUnhandledPacket(string packetId)
    {
        if (!_rwConfig.UnhandledPackets.Contains(packetId))
            return;

        var packets = _rwConfig.UnhandledPackets.ToList();
        packets.Remove(packetId);
        _rwConfig.UnhandledPackets = [.. packets];
    }

    public virtual void Dispose()
    {
        if (Socket == null || _disposing)
            return;

        _disposing = true;

        try
        {
            Socket.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException ex)
        {
            _handler.TraceNetworkError(ex, this);
        }

        try
        {
            Socket.Close();
        }
        catch (SocketException ex)
        {
            _handler.TraceNetworkError(ex, this);
        }

        _sink.InvokeNetStateRemoved(new NetStateRemovedEventArgs(this));

        Socket = null;
        _onReceiveCallback = null;
        _onSendCallback = null;
        Throttler = null;

        _data.Clear();

        Running = false;

        lock (_handler.Disposed)
            _handler.Disposed.Enqueue(this);

        GC.SuppressFinalize(this);
    }
}
