using A2m.Server;

namespace Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;

public interface IDamageable
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; }
    public int GetDamageAmount(int damage, Elemental damageType);
}
