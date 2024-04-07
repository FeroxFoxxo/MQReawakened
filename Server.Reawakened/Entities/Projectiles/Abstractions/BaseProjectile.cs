using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles.Abstractions;
public abstract class BaseProjectile(string id, float speedX, float speedY, float lifetime,
    Room room, Vector3Model position, Vector3Model endPosition, bool gravity, ServerRConfig config)
{
    public string ProjectileId => id;
    public Room Room => room;

    public readonly string PrjPlane = position.Z > 10 ? config.FrontPlane : config.BackPlane;

    public Vector3 Position = new() { x = position.X, y = position.Y, z = position.Z };
    public Vector3 SpawnPosition = new() { x = position.X, y = position.Y, z = position.Z };

    public Vector3 Speed = new(speedX, speedY, 1);

    public float StartTime = room.Time;
    public float LifeTime = room.Time + lifetime;

    public BaseCollider Collider { get; set; }

    public void Update()
    {
        if (room == null) return;

        if (Position.y <= endPosition?.Y)
            Hit("-1");

        Move();

        var time = room.Time;

        var collisions = Collider.IsColliding(true);

        if (collisions.Length > 0)
            foreach (var collision in collisions)
                Hit(collision);

        if (LifeTime <= time)
            Hit("-1");
    }

    public virtual void Move()
    {
        Position = GetPositionBasedOnTime(SpawnPosition);
        Collider.Position = GetPositionBasedOnTime(Collider.SpawnPosition);
    }

    private Vector3 GetPositionBasedOnTime(Vector3 spawnPos)
    {
        var timeDelta = Room.Time - StartTime;
        var newPosition = spawnPos + timeDelta * Speed;

        if (gravity)
            newPosition.y = timeDelta * (spawnPos.y + Speed.y - 0.5f * config.Gravity * timeDelta);

        return newPosition;
    }

    public abstract void Hit(string hitGoID);

    public void DecreaseSpeedY(float speed) => speedY -= speed;
}
