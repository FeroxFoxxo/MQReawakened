using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Timer = Server.Base.Timers.Timer;

namespace Server.Reawakened.Levels;

public class LevelTimer : Timer
{
    private readonly LevelEntities _entities;

    public LevelTimer(ServerStaticConfig config, TimerThread timerThread, LevelEntities entities) :
        base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / config.LevelTickRate), 0, timerThread) =>
        _entities = entities;

    public override void OnTick()
    {
        foreach (var entity in _entities.Entities.Values.SelectMany(entityList => entityList))
            entity.Update();
    }
}
