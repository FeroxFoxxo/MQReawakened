using Server.Base.Core.Extensions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Players.Models.Groups;
using Server.Reawakened.Players.Models.Trade;
using UnityEngine;
using Timer = Server.Base.Timers.Timer;

namespace Server.Reawakened.Players.Models.Misc;

public class TemporaryDataModel
{
    public string GameObjectId { get; set; } = "0";
    public int Direction { get; set; } = 0;
    public int Locale { get; set; }

    public bool PetDefense { get; set; }
    public bool PetDefensiveBarrier { get; set; }
    public bool Invincible { get; set; } = false;
    public bool OnGround { get; set; } = false;
    public bool Underwater { get; set; } = false;
    public Timer UnderwaterTimer { get; set; } = null;
    public Timer PetEnergyRegenTimer { get; set; } = null;
    public bool BananaBoostsElixir { get; set; }
    public bool ReputationBoostsElixir { get; set; }
    public bool IsSuperStomping { get; set; } = false;
    public bool IsSlowed { get; set; } = false;
    public TriggerArenaComp CurrentArena { get; set; } = null;

    public List<string> CollidingHazards { get; set; } = [];
    public Dictionary<int, bool> VotedForItem { get; set; } = [];

    public Vector3 Position { get; set; } = new Vector3();
    public Vector3 Velocity { get; set; } = new Vector3();

    public TradeModel TradeModel { get; set; }
    public GroupModel Group { get; set; }

    public Dictionary<int, List<string>> CurrentAchievements { get; set; } = [];

    public bool FirstLogin { get; set; } = true;
    public long CurrentPing { get; set; } = GetTime.GetCurrentUnixMilliseconds();

    public Vector3 CopyPosition() =>
        new(Position.x, Position.y, Position.z);
}
