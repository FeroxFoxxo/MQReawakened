using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Worlds.Services;

public class AutoSave : IService
{
    
    private readonly TimeSpan _delayAutoSave;
    private readonly TimeSpan _delayWarning;

    private readonly ServerHandler _handler;
    private readonly EventSink _sink;
    private readonly TimerThread _timerThread;
    private readonly World _world;

    public AutoSave(InternalRConfig config, ServerHandler handler, World world, EventSink sink, TimerThread timerThread)
    {
        _handler = handler;
        _world = world;
        _sink = sink;
        _timerThread = timerThread;

        _delayAutoSave = TimeSpan.FromMinutes(config.SaveAutomaticallyMinutes);
        _delayWarning = TimeSpan.FromMinutes(config.SaveWarningMinutes);
    }

    public void Initialize() => _sink.ServerStarted += _ => RunAutoSaveTimer();

    private void RunAutoSaveTimer()
    {
        var timer = _timerThread.DelayCall(Tick, _delayAutoSave - _delayWarning, _delayAutoSave, 0);
        timer.Stop();
    }

    public void Save() => _world.Save(true, false);

    private void Tick()
    {
        if (_handler.Restarting)
            return;

        if (_delayWarning == TimeSpan.Zero)
        {
            Save();
        }
        else
        {
            var seconds = (int)_delayWarning.TotalSeconds;
            var minutes = seconds / 60;
            seconds %= 60;

            switch (minutes)
            {
                case > 0 when seconds > 0:
                    _world.Broadcast(
                        $"The world will save in {minutes} minute{(minutes != 1 ? "s" : string.Empty)} and {seconds} second{(seconds != 1 ? "s" : string.Empty)}.");
                    break;
                case > 0:
                    _world.Broadcast($"The world will save in {minutes} minute{(minutes != 1 ? "s" : string.Empty)}.");
                    break;
                default:
                    _world.Broadcast($"The world will save in {seconds} second{(seconds != 1 ? "s" : string.Empty)}.");
                    break;
            }

            _timerThread.DelayCall(Save, _delayWarning, TimeSpan.Zero, 1);
        }
    }
}
