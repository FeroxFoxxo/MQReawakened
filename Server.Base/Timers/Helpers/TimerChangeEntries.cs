namespace Server.Base.Timers.Helpers;

public class TimerChangeEntry(Timer timer, int newIndex, bool adding, TimerChangePool pool)
{
    private readonly TimerChangePool _pool = pool;
    public bool Adding = adding;

    public int Index = newIndex;
    public Timer Timer = timer;

    public void Free()
    {
        Timer = null;

        lock (_pool.InstancePool)
        {
            if (_pool.InstancePool.Count < 512)
                _pool.InstancePool.Enqueue(this);
        }
    }
}
