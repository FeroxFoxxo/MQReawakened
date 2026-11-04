using A2m.Server;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.GameObjects.InterObjs.Abstractions;

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

    public override void InitializeComponent() => CurrentHealth = MaxHealth;

    public int GetDamageAmount(int damage, ItemEffectType itemEffectType, bool applyResist = true)
    {
        if (applyResist)
        {
            switch (itemEffectType)
            {
                case ItemEffectType.AirDamage:
                    damage -= AirDamageResistPoints;
                    break;
                case ItemEffectType.FireDamage:
                    damage -= FireDamageResistPoints;
                    break;
                case ItemEffectType.IceDamage:
                    damage -= IceDamageResistPoints;
                    break;
                case ItemEffectType.EarthDamage:
                    damage -= EarthDamageResistPoints;
                    break;
                case ItemEffectType.PoisonDamage:
                    damage -= PoisonDamageResistPoints;
                    break;
                case ItemEffectType.LightningDamage:
                    damage -= LightningDamageResistPoints;
                    break;
                default:
                    damage -= StandardDamageResistPoints;
                    break;
            }
        }

        if (damage < 1)
            damage = 1;

        return damage;
    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }
}
