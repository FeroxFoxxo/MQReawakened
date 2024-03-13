using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.AbstractComponents;

public abstract class BaseBreakableObjStatusComp<T> : BaseInterObjStatusComp<T>, IBreakable where T : BreakableObjStatus
{
    public string OnKillMessageReceiver => ComponentData.OnKillMessageReceiver;
    public int NbOfHitToBreak => ComponentData.NbOfHitToBreak;
    public bool EnemyTarget => ComponentData.EnemyTarget;
    public bool LocalCollision => ComponentData.LocalCollision;

    public int NumberOfHitsToBreak => NbOfHitToBreak;

    public override object[] GetInitData(Player player) => [ CurrentHealth, MaxHealth, GenericLevel ];
}
