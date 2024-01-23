using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Stats;

public class BreakableObjStatusComp : Component<BreakableObjStatus>
{
    public string OnKillMessageReceiver => ComponentData.OnKillMessageReceiver;
    public int NbOfHitToBreak => ComponentData.NbOfHitToBreak;
    public bool EnemyTarget => ComponentData.EnemyTarget;
    public bool LocalCollision => ComponentData.LocalCollision;
}
