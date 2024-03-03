﻿using A2m.Server;

namespace Server.Reawakened.Entities.AbstractComponents;

public interface IDamageable
{
    public int MaxHealth { get; }
    public int GetDamageAmount(int damage, Elemental damageType);
}