using Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Abstractions;
using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.GameObjects.Breakables.Abstractions;

public abstract class BaseBreakableObjStatusComp<T> : BaseInterObjStatusComp<T>, IBreakable where T : BreakableObjStatus
{
    public string OnKillMessageReceiver => ComponentData.OnKillMessageReceiver;
    public int NbOfHitToBreak => ComponentData.NbOfHitToBreak;
    public bool EnemyTarget => ComponentData.EnemyTarget;
    public bool LocalCollision => ComponentData.LocalCollision;

    public int NumberOfHitsToBreak => NbOfHitToBreak;
    public int NumberOfHits { get; set; } = 0;

    public override object[] GetInitData(Player player) => [CurrentHealth, MaxHealth, GenericLevel];
}
