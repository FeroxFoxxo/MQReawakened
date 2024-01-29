using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity;
public class TicklyEntity : Component<ProjectileController>
{
    public new Vector3Model Position;
    public float Speed, LifeTime, StartTime;
    public Player Player;
    public string ProjectileID = string.Empty;
    public string PrjPlane;
    public BaseCollider Collider;
    public double Tickrate;
    public ILogger<TicklyEntity> Logger { get; set; }

    public override void Update()
    {
        if (Speed != 0)
        {
            Position.X += Speed / (float)(Tickrate - 2);
            Collider.Position.x = Position.X;
        }

        var Collisions = Collider.IsColliding(true);
        if (Collisions.Length > 0)
            foreach (var collision in Collisions)
            {
                Hit(collision);
            }

        if (LifeTime <= Player.Room.Time)
            Hit("-1");
    }

    public virtual void Hit(string hitGoID)
    {
    }
}
