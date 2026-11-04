using Server.Base.Core.Extensions;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Players.Models.Groups;
using Server.Reawakened.Players.Models.Trade;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;
using Timer = Server.Base.Timers.Timer;

namespace Server.Reawakened.Players.Models.Misc;

public class TemporaryDataModel
{
    public int ProjectileId { get; set; } = -1;
    public string GameObjectId { get; set; } = "0";
    public string CurrentCoopTriggerId { get; set; } = "0";
    public int Direction { get; set; } = 0;
    public int Locale { get; set; }

    public bool OnGround { get; set; } = false;
    public bool Underwater { get; set; } = false;
    public float UnderwaterTime { get; set; } = 0;
    public Timer PoisonEffectTimer { get; set; } = null;
    public bool BananaBoostsElixir { get; set; }
    public bool ReputationBoostsElixir { get; set; }
    public bool IsSuperStomping { get; set; } = false;
    public bool IsPoisoned { get; set; } = false;
    public bool IsSlowed { get; set; } = false;
    public BaseComponent CurrentArena { get; set; } = null;
    public bool IsKnockedOut { get; set; } = false;

    public Dictionary<int, bool> VotedForItem { get; set; } = [];
    public Dictionary<float, BaseEnemy> EnemiesInPetAbilityZone { get; set; }

    public Vector3Model Position { get; set; } = new Vector3Model(0, 0, 0);
    public Vector3 Velocity { get; set; } = new Vector3();

    public TradeModel TradeModel { get; set; }
    public GroupModel Group { get; set; }

    public Dictionary<int, List<string>> CurrentAchievements { get; set; } = [];

    public bool FirstLogin { get; set; } = true;
    public long CurrentPing { get; set; } = GetTime.GetCurrentUnixMilliseconds();

    public PlayerCollider PlayerCollider { get; set; }

    public DateTime LastPetEnergyRegen { get; set; }

    public Vector3Model CopyPosition() =>
        new(Position.X, Position.Y, Position.Z);
}
