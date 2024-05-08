using A2m.Server;

namespace Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;

public interface IDamageable
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; }
    public int Stars { get; }
    public int DifficultyLevel { get; }

    public int GetDamageAmount(int damage, Elemental damageType);
}
