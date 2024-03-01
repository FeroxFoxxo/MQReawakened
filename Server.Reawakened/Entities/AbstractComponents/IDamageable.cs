using A2m.Server;

namespace Server.Reawakened.Entities.AbstractComponents;

public interface IDamageable
{
    public int GetDamageType(int damage, Elemental damageType);
}
