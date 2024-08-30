using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Timers;

public class DelayCallTimer(TimeSpan delay, TimeSpan interval, int count, Timer.TimerCallback callback, ITimerData data, TimerThread tThread) : Timer(delay, interval, count, tThread)
{
    public TimerCallback Callback => callback;

    public override void OnTick()
    {
        if (Callback == null)
            return;

        if (data == null)
            return;

        if (!data.IsValid())
            return;

        Callback?.Invoke(data);
    }

    public override string ToString() => $"DelayCallTimer [{Callback.FormatDelegate()}]";
}
