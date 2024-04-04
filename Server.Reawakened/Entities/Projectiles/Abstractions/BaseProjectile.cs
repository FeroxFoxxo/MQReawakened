using Server.Reawakened.Configs;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Projectiles.Abstractions;
public abstract class BaseProjectile(string id, float speedX, float speedY, float tickDuration,
    Room room, Vector3Model position, Vector3Model endPosition, ServerRConfig config)
{
    public string ProjectileId => id;
    public Room Room => room;
    public Vector3Model Position => position;
    public float SpeedX => speedX;
    public float SpeedY => speedY;

    public readonly string PrjPlane = position.Z > 10 ? config.FrontPlane : config.BackPlane;

    public Vector3Model SpawnPosition = new() { X = position.X, Y = position.Y, Z = position.Z };

    public float StartTime = room.Time;
    public float LifeTime = room.Time + tickDuration;

    public abstract BaseCollider Collider { get; set; }

    public void Update()
    {
        if (room == null) return;

        if (position.Y <= endPosition?.Y)
            Hit("-1");

        Move();

        var collisions = Collider.IsColliding(true);

        if (collisions.Length > 0)
            foreach (var collision in collisions)
                Hit(collision);

        if (LifeTime <= room.Time)
            Hit("-1");
    }

    public virtual void Move()
    {
        var newX = (room.Time - StartTime) * speedX;
        position.X = SpawnPosition.X + newX;
        Collider.Position.x = Collider.SpawnPosition.x + newX;

        var newY = (room.Time - StartTime) * speedY;
        position.Y = SpawnPosition.Y + newY;
        Collider.Position.y = Collider.SpawnPosition.y + newY;
    }

    public abstract void Hit(string hitGoID);

    public void DecreaseSpeedY(float speed) => speedY -= speed;
}
