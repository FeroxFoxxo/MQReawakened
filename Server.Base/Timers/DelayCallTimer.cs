using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Timers;

public class DelayCallTimer(TimeSpan delay, TimeSpan interval, int count, Timer.TimerCallback callback, object data, TimerThread tThread) : Timer(delay, interval, count, tThread)
{
    public TimerCallback Callback => callback;

    public override void OnTick() => Callback?.Invoke(data);

    public override string ToString() => $"DelayCallTimer [{Callback.FormatDelegate()}]";
}
