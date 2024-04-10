using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles.Abstractions;
public abstract class BaseProjectile(string id, float lifetime,
    Room room, Vector3 position, Vector2 speed, Vector3? endPosition, bool gravity, ServerRConfig config)
{
    public string ProjectileId => id;
    public Room Room => room;

    public readonly string PrjPlane = position.z > 10 ? config.FrontPlane : config.BackPlane;

    public Vector3 Position = new() { x = position.x, y = position.y, z = position.z };
    public Vector3 SpawnPosition = new() { x = position.x, y = position.y, z = position.z };

    public Vector3 Speed = new(speed.x, speed.y, 1);

    public float StartTime = room.Time;
    public float LifeTime = room.Time + lifetime;

    public BaseCollider Collider { get; set; }

    public void Update()
    {
        if (room == null) return;

        if (endPosition.HasValue)
            if (Position.y <= endPosition.Value.y)
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

        var pos = new Vector3()
        {
            x = GetCoordFromTime(spawnPos.x, Speed.x, timeDelta),
            y = GetCoordFromTime(spawnPos.y, Speed.y, timeDelta),
            z = spawnPos.z
        };

        if (gravity)
            pos.y -= 0.5f * 15f * timeDelta * timeDelta;

        return pos;
    }

    private static float GetCoordFromTime(float spawnCoord, float speedCoord, float timeDelta) =>
        spawnCoord + timeDelta * speedCoord;

    public abstract void Hit(string hitGoID);

    public void DecreaseSpeedY(float speedY) => Speed.y -= speedY;
}
