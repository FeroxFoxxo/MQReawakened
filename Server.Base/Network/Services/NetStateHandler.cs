using Server.Base.Accounts.Models;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Base.Network.Enums;
using Server.Base.Network.Models;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Network.Services;

public class NetStateHandler(FileLogger fileLogger, TimerThread thread,
    EventSink sink) : IService
{
    public delegate ProtocolResponse GetProtocol(string protocol);
    public delegate void SendProtocol(NetState netState, string actionType, object protocol);

    public readonly Queue<NetState> Disposed = new();
    public readonly List<NetState> Instances = [];

    public readonly Dictionary<char, GetProtocol> ProtocolLookup = [];
    public readonly Dictionary<char, SendProtocol> ProtocolSend = [];

    public bool Paused = false;

    public void Initialize() =>
        sink.ServerStarted += _ =>
            thread.DelayCall(CheckAllAlive, TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.5), 0);

    public NetState FindUser(int userId) =>
        (from state in Instances
         let account = state.Get<Account>()
         where account != null
         where account.Id == userId
         select state
    ).FirstOrDefault();

    public void ProcessDisposedQueue()
    {
        lock (Disposed)
        {
            var breakout = 200;

            while (--breakout >= 0 && Disposed.Count > 0)
            {
                var netState = Disposed.Dequeue();

                Instances.Remove(netState);

                netState.RemoveAllData();
            }
        }
    }

    public void Pause()
    {
        Paused = true;

        foreach (var ns in Instances)
        {
            lock (ns.AsyncLock)
                ns.AsyncState |= AsyncStates.Paused;
        }
    }

    public void Resume()
    {
        Paused = false;

        foreach (var ns in Instances.Where(ns => ns.Socket != null))
        {
            lock (ns.AsyncLock)
            {
                ns.AsyncState &= ~AsyncStates.Paused;

                try
                {
                    if ((ns.AsyncState & AsyncStates.Pending) == 0)
                        ns.BeginReceive();
                }
                catch (Exception ex)
                {
                    TraceNetworkError(ex, ns);
                    ns.Dispose();
                }
            }
        }
    }

    public void CheckAllAlive()
    {
        var curTicks = GetTicks.Ticks;

        var instanceCount = Instances.Count;

        while (--instanceCount >= 0)
        {
            var instance = Instances[instanceCount];

            if (instance == null)
                continue;

            try
            {
                instance.CheckAlive(curTicks);
            }
            catch (Exception ex)
            {
                TraceNetworkError(ex, instance);
            }
        }
    }

    public void TraceNetworkError(Exception ex, NetState state) =>
        fileLogger.WriteGenericLog<NetState>("network-errors", $"Client {state}", ex.ToString(), LoggerType.Error);
}
