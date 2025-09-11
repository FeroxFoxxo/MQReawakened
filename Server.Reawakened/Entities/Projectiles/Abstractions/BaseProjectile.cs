using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles.Abstractions;
public abstract class BaseProjectile(string id, float lifetime,
    Room room, Vector3Model position, Vector2 speed, Vector3? endPosition, bool gravity)
{
    public string ProjectileId => id;
    public Room Room => room;

    public string PrjPlane => position.Z.GetPlaneFromZ();

    public Vector3Model Position => position;
    public Vector3 SpawnPosition = new() { x = position.X, y = position.Y, z = position.Z };

    public Vector3 Speed = new(speed.x, speed.y, 1);

    public float StartTime = room.Time;
    public float LifeTime = room.Time + lifetime;

    public BaseCollider Collider { get; set; }

    public void Update()
    {
        if (room == null) return;

        if (endPosition.HasValue)
            if (Position.Y <= endPosition.Value.y)
                Hit("-1");

        Move();

        var time = room.Time;

        var collisions = Collider.RunCollisionDetection();

        if (collisions.Length > 0)
            foreach (var collision in collisions)
                Hit(collision);

        if (LifeTime <= time)
            Hit("-1");
    }

    public virtual void Move() => SetPositionBasedOnTime();

    private void SetPositionBasedOnTime()
    {
        var timeDelta = Room.Time - StartTime;

        var pos = new Vector3()
        {
            x = GetCoordFromTime(SpawnPosition.x, Speed.x, timeDelta),
            y = GetCoordFromTime(SpawnPosition.y, Speed.y, timeDelta),
            z = SpawnPosition.z
        };

        if (gravity)
            pos.y -= 0.5f * 15f * timeDelta * timeDelta;

        Position.SetPosition(pos);
    }

    private static float GetCoordFromTime(float spawnCoord, float speedCoord, float timeDelta) =>
        spawnCoord + timeDelta * speedCoord;

    public abstract void Hit(string hitGoID);

    public void DecreaseSpeedY(float speedY) => Speed.y -= speedY;
}
