using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Timers.Helpers;
using Server.Base.Worlds;
using System.Globalization;

namespace Server.Base.Timers.Services;

public class TimerThread : IService
{
    private readonly Dictionary<Timer, TimerChangeEntry> _changed;
    private readonly InternalRConfig _config;
    private readonly ServerHandler _handler;
    private readonly double[] _nextPriorities;
    private readonly TimerChangePool _pool;
    private readonly Queue<Timer> _queue;
    private readonly EventSink _sink;
    private readonly List<Timer>[] _timers;
    private readonly Thread _timerThread;
    private readonly World _world;
    private readonly AutoResetEvent _signal;
    private readonly ILogger<TimerThread> _logger;

    public TimerThread(InternalRConfig config, TimerChangePool pool, EventSink sink, ServerHandler handler, World world, ILogger<TimerThread> logger)
    {
        _config = config;
        _pool = pool;
        _sink = sink;
        _handler = handler;
        _changed = [];
        _queue = new Queue<Timer>();
        _world = world;
        _signal = new AutoResetEvent(false);
        _logger = logger;

        _nextPriorities = [.. Enumerable.Repeat(default(double), config.Delays.Length)];
        _timers = [.. Enumerable.Repeat(new List<Timer>(), config.Delays.Length)];

        _timerThread = new Thread(RunTimer)
        {
            Name = "Timer Thread",
            CurrentCulture = CultureInfo.InvariantCulture
        };
    }

    public void Initialize() => _sink.ServerStarted += _ => _timerThread.Start();

    public void AddTimer(Timer timer) => Change(timer, (int)timer.Priority, true);

    public void PriorityChange(Timer timer, int newPriority) => Change(timer, newPriority, false);

    public void RemoveTimer(Timer timer) => Change(timer, -1, false);

    public void Change(Timer timer, int index, bool adding)
    {
        lock (_changed)
            _changed[timer] = _pool.GetInstance(timer, index, adding);

        _signal.Set();
    }

    private void ProcessChanged()
    {
        lock (_changed)
        {
            var curTicks = GetTicks.TickCount;

            foreach (var tce in _changed.Values)
            {
                var timer = tce.Timer;
                var newIndex = tce.Index;

                timer.List?.Remove(timer);

                if (tce.Adding)
                {
                    timer.Next = curTicks + timer.Delay;
                    timer.Index = 0;
                }

                if (newIndex >= 0)
                {
                    timer.List = _timers[newIndex];
                    timer.List.Add(timer);
                }
                else
                {
                    timer.List = null;
                }

                tce.Free();
            }

            _changed.Clear();
        }
    }

    public void Slice()
    {
        lock (_queue)
        {
            var index = 0;

            while (index < _config.BreakCount && _queue.Count != 0)
            {
                var t = _queue.Dequeue();

                try
                {
                    t.OnTick();
                }
                catch (Exception e)
                {
                    _logger.LogError("Timer {TIMER} threw an exception {Message}.", t.Index, e.Message);
                }

                t.Queued = false;
                ++index;
            }
        }
    }

    public void RunTimer()
    {
        long now;
        int i, j;

        while (!_handler.IsClosing)
        {
            if (_world.Loading || _world.Saving)
            {
                _signal.WaitOne(1, false);
                continue;
            }

            var loaded = false;
            ProcessChanged();

            for (i = 0; i < _timers.Length; i++)
            {
                now = GetTicks.TickCount;

                if (now < _nextPriorities[i])
                    break;

                _nextPriorities[i] = now + _config.Delays[i];

                for (j = 0; j < _timers[i].Count; j++)
                {
                    var t = _timers[i][j];

                    if (t.Queued || now <= t.Next)
                        continue;

                    t.Queued = true;

                    lock (_queue)
                        _queue.Enqueue(t);

                    loaded = true;

                    if (t.Count != 0 && ++t.Index >= t.Count)
                        t.Stop();
                    else
                        t.Next = now + t.Interval;
                }
            }

            if (loaded)
                _handler.Set();

            _signal.WaitOne(1, false);
        }
    }
}
