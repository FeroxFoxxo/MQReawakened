namespace Server.Base.Timers.Helpers;

public class TimerChangeEntry(Timer timer, int newIndex, bool adding, TimerChangePool pool)
{
    public bool Adding = adding;
    public Timer Timer = timer;
    public int Index = newIndex;

    public void Free()
    {
        Timer = null;

        lock (pool.InstancePool)
        {
            if (pool.InstancePool.Count < 512)
                pool.InstancePool.Enqueue(this);
        }
    }
}
