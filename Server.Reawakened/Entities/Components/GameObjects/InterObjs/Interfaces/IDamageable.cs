using A2m.Server;

namespace Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;

public interface IDamageable
{
    int CurrentHealth { get; set; }
    int MaxHealth { get; }
    int Stars { get; }
    int DifficultyLevel { get; }

    int GetDamageAmount(int damage, Elemental damageType);
}
