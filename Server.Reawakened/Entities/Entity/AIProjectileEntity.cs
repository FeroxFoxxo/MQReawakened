﻿using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity;
public class AIProjectileEntity : TicklyEntity
{
    private Vector3Model _hitboxPosition;
    private string _ownerId;
    private float SpeedY;
    private Room _room;

    public AIProjectileEntity(Room room, string ownerId, string id, Vector3Model position, float speedX, float speedY, float lifeTime)
    {
        // Initialize projectile location info
        _room = room;
        _ownerId = ownerId;
        ProjectileID = id;
        Position = position;
        PrjPlane = Position.Z > 10 ? "Plane1" : "Plane0";
        SpawnPosition = new Vector3Model { X = Position.X, Y = Position.Y, Z = Position.Z };

        // Initialize projectile info
        Speed = speedX;
        SpeedY = speedY;
        StartTime = _room.Time;
        LifeTime = StartTime + lifeTime;

        // Send all information to room
        Collider = new AIProjectileCollider(id, room, ProjectileID, Position, 0.5f, 0.5f, PrjPlane, LifeTime);
    }

    public override void Update()
    {
        Position.X = SpawnPosition.X + (_room.Time - StartTime) * Speed;
        Collider.Position.x = Position.X;

        Position.Y = SpawnPosition.Y + (_room.Time - StartTime) * SpeedY;
        Collider.Position.y = Position.Y;

        var Collisions = Collider.IsColliding(true);
        if (Collisions.Length > 0)
            foreach (var collision in Collisions)
            {
                Console.WriteLine(collision);
                Hit(collision);
            }

        if (LifeTime <= _room.Time)
            Hit("-1");
    }

    public override void Hit(string hitGoID)
    {
        //Logger.LogInformation("Projectile with ID {args1} destroyed at position ({args2}, {args3}, {args4})", ProjectileID, Position.X, Position.Y, Position.Z);
        var hit = new ProjectileHit_SyncEvent(new SyncEvent(_ownerId, SyncEvent.EventType.ProjectileHit, _room.Time));
        hit.EventDataList.Add(int.Parse(ProjectileID));
        hit.EventDataList.Add(hitGoID);
        hit.EventDataList.Add(0);
        hit.EventDataList.Add(Position.X);
        hit.EventDataList.Add(Position.Y);

        _room.SendSyncEvent(hit);
        _room.Projectiles.Remove(ProjectileID);
    }
}