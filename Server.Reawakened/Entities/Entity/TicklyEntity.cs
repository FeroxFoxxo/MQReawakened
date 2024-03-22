using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Entity;
public class TicklyEntity : Component<ProjectileController>
{
    public new Vector3Model Position;
    public Vector3Model SpawnPosition;
    public Vector3Model ChargeEndPosition;

    public float SpeedX, SpeedY, LifeTime, StartTime;
    public bool IsGrenade;

    public Player Player;

    public string ProjectileID = string.Empty;
    public string PrjPlane;

    public BaseCollider Collider;

    public ServerRConfig ServerRConfig;
    public ILogger<TicklyEntity> Logger { get; set; }

    public override void Update()
    {
        if (Player == null) return;
        if (Player.Room == null) return;

        if (Position.Y <= ChargeEndPosition?.Y)
            Hit("-1");

        if (IsGrenade)
            SpeedY -= ServerRConfig.GrenadeGravityFactor;

        if (SpeedX != 0 || SpeedY != 0)
        {
            Position.X = SpawnPosition.X + (Player.Room.Time - StartTime) * SpeedX;
            Collider.Position.x = Collider.SpawnPosition.x + (Player.Room.Time - StartTime) * SpeedX;

            Position.Y = SpawnPosition.Y + (Player.Room.Time - StartTime) * SpeedY;
            Collider.Position.y = Collider.SpawnPosition.y + (Player.Room.Time - StartTime) * SpeedY;
        }

        var Collisions = Collider.IsColliding(true);

        if (Collisions.Length > 0)
            foreach (var collision in Collisions)
                Hit(collision);

        if (LifeTime <= Player.Room.Time)
            Hit("-1");
    }

    public virtual void Hit(string hitGoID)
    {
    }
}
