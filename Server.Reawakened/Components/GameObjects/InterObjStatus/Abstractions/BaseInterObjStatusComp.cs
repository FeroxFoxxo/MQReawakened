using A2m.Server;
using Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.AbstractComponents;

public abstract class BaseInterObjStatusComp<T> : Component<T>, IDamageable where T : InterObjStatus
{
    public int DifficultyLevel => ComponentData.DifficultyLevel;
    public int GenericLevel => ComponentData.GenericLevel;
    public int Stars => ComponentData.Stars;
    public int MaxHealth => ComponentData.MaxHealth;
    public float LifebarOffsetX => ComponentData.LifeBarOffsetX;
    public float LifebarOffsetY => ComponentData.LifeBarOffsetY;
    public string EnemyLifeBar => ComponentData.EnemyLifeBar;

    public int StandardDamageResistPoints => ComponentData.StandardDamageResistPoints;
    public int FireDamageResistPoints => ComponentData.FireDamageResistPoints;
    public int IceDamageResistPoints => ComponentData.IceDamageResistPoints;
    public int PoisonDamageResistPoints => ComponentData.PoisonDamageResistPoints;
    public int LightningDamageResistPoints => ComponentData.LightningDamageResistPoints;

    public int AirDamageResistPoints => ComponentData.AirDamageResistPoints;
    public int EarthDamageResistPoints => ComponentData.EarthDamageResistPoints;
    public int StunStatusEffectResistSecs => ComponentData.StunStatusEffectResistSecs;
    public int SlowStatusEffectResistSecs => ComponentData.SlowStatusEffectResistSecs;
    public int FreezeStatusEffectResistSecs => ComponentData.FreezeStatusEffectResistSecs;

    public int CurrentHealth { get; set; }

    public int GetDamageAmount(int damage, Elemental damageType)
    {
        switch (damageType)
        {
            case Elemental.Air:
                damage -= AirDamageResistPoints;
                break;
            case Elemental.Fire:
                damage -= FireDamageResistPoints;
                break;
            case Elemental.Ice:
                damage -= IceDamageResistPoints;
                break;
            case Elemental.Earth:
                damage -= EarthDamageResistPoints;
                break;
            case Elemental.Poison:
                damage -= PoisonDamageResistPoints;
                break;
            case Elemental.Standard:
            case Elemental.Unknown:
            case Elemental.Invalid:
                damage -= StandardDamageResistPoints;
                break;
        }

        if (damage < 0)
            damage = 0;

        return damage;
    }
}
