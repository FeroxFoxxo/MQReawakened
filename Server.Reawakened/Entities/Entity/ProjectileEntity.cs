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
public class ProjectileEntity : Component<ProjectileController>
{
    public new Vector3Model Position;
    public float Speed, LifeTime, StartTime;
    public Player Player;
    public string ProjectileID = string.Empty;
    private readonly string _plane;
    public BaseCollider PrjCollider;
    public double Tickrate;
    public ILogger<ProjectileEntity> Logger { get; set; }

    public ProjectileEntity(Player player, string id, float posX, float posY, float posZ, int direction, float lifeTime, ItemDescription item, int damage, Elemental type, ServerRConfig config)
    {
        // Initialize projectile info
        var isLeft = direction > 0;
        posX += isLeft ? config.ProjectileXOffset : -config.ProjectileXOffset;
        posY += config.ProjectileYOffset;
        Speed = isLeft ? config.ProjectileSpeed : -config.ProjectileSpeed;
        StartTime = player.Room.Time;
        LifeTime = StartTime + lifeTime;

        // Initialize projectile location info
        Tickrate = config.RoomTickRate;
        Player = player;
        ProjectileID = id;
        Position = new Vector3Model{ X = posX, Y = posY, Z = posZ };
        _plane = Position.Z > 10 ? "Plane1" : "Plane0";

        // Send all information to room
        PrjCollider = new AttackCollider(id, Position, config.ProjectileWidth, config.ProjectileHeight, _plane, player, damage, type, LifeTime);
        var prj = new LaunchItem_SyncEvent(player.GameObjectId.ToString(), StartTime, posX, posY, posZ, Speed, 0, LifeTime, int.Parse(ProjectileID), item.PrefabName);
        player.Room.SendSyncEvent(prj);
    }

    public override void Update()
    {
        Position.X += Speed / (float)Tickrate;
        PrjCollider.Position.x = Position.X;

        var Collisions = PrjCollider.IsColliding(true);
        if (Collisions.Length > 0)
            foreach (var collision in Collisions)
            {
                ProjectileHit(collision);
            }

        if (LifeTime <= Player.Room.Time)
            ProjectileHit("-1");
    }

    public void ProjectileHit(string hitGoID)
    {
        //Logger.LogInformation("Projectile with ID {args1} destroyed at position ({args2}, {args3}, {args4})", ProjectileID, Position.X, Position.Y, Position.Z);
        var hit = new ProjectileHit_SyncEvent(new SyncEvent(Player.GameObjectId, SyncEvent.EventType.ProjectileHit, Player.Room.Time));
        hit.EventDataList.Add(int.Parse(ProjectileID));
        hit.EventDataList.Add(hitGoID);
        hit.EventDataList.Add(0);
        hit.EventDataList.Add(Position.X);
        hit.EventDataList.Add(Position.Y);

        Player.Room.SendSyncEvent(hit);
        Player.Room.Projectiles.Remove(ProjectileID);
    }
}
