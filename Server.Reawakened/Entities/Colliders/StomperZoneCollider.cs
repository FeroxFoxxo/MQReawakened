using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Rooms;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class StomperZoneCollider(string id, Vector3 position,
    Rect box, string plane, Room room, bool hazard, TimerThread timerThread, ServerRConfig config) :
    BaseCollider(id, position, box, plane, room, ColliderType.Stomper)
{

    public bool Hazard = hazard;
    public TimerThread TimerThread = timerThread;
    public ServerRConfig ServerRConfig = config;

    public override string[] IsColliding()
    {
        var colliders = Room.GetColliders();

        List<string> collidedWith = [];

        foreach (var collider in colliders)
        {
            var collided = CheckCollision(collider);
            var isCollidableType = collider.Type is ColliderType.Player;

            if (collided && isCollidableType)
            {
                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }
        }

        return [.. collidedWith];
    }
}
