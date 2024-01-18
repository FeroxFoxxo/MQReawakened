using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class InterObjStatusComp : Component<InterObjStatus>
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

    public ILogger<InterObjStatusComp> Logger { get; set; }

}
