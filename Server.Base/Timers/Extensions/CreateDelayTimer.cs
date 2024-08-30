using Server.Base.Core.Abstractions;
using Server.Base.Timers.Services;

namespace Server.Base.Timers.Extensions;

public static class CreateDelayTimer
{
    public static Timer RunInstantly(this TimerThread thread, Timer.TimerCallback callback, ITimerData data) =>
        thread.DelayCall(callback, data, TimeSpan.Zero, TimeSpan.Zero, 1);

    public static Timer RunDelayed(this TimerThread thread, Timer.TimerCallback callback, ITimerData data, TimeSpan delay) =>
        thread.DelayCall(callback, data, delay, TimeSpan.Zero, 1);

    public static Timer RunInterval(this TimerThread thread, Timer.TimerCallback callback, ITimerData data, TimeSpan interval, int count, TimeSpan initialDelay) =>
        thread.DelayCall(callback, data, initialDelay, interval, count);

    public static Timer RunIndefiniteDelayedInterval(this TimerThread thread, Timer.TimerCallback callback, ITimerData data, TimeSpan interval, TimeSpan delay) =>
        thread.DelayCall(callback, data, delay, interval, 0);

    private static Timer DelayCall(this TimerThread thread, Timer.TimerCallback callback, ITimerData data, TimeSpan delay,
        TimeSpan interval, int count)
    {
        Timer timer = new DelayCallTimer(delay, interval, count, callback, data, thread)
        {
            Priority = (count == 1 ? delay : interval).ComputePriority()
        };

        timer.Start();

        return timer;
    }
}
