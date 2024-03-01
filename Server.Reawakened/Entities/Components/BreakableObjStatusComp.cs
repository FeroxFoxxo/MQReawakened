using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;
public class BreakableObjStatusComp : BaseInterObjStatusComp<BreakableObjStatus>
{
    public string OnKillMessageReceiver => ComponentData.OnKillMessageReceiver;
    public int NbOfHitToBreak => ComponentData.NbOfHitToBreak;
    public bool EnemyTarget => ComponentData.EnemyTarget;
    public bool LocalCollision => ComponentData.LocalCollision;
}
