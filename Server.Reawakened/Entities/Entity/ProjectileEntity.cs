using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Entity;

public class ProjectileEntity : Component<ProjectileController>
{
    public int ProjectileID = 0;

    public new Vector3Model Position;
    public float Speed, LifeTime, StartTime;
    private readonly string _plane;

    public BaseCollider PrjCollider;

    public Player Player;

    public ILogger<ProjectileEntity> Logger { get; set; }

    public ProjectileEntity(Player player, int id, float posX, float posY, float posZ, int direction, float lifeTime, ItemDescription item)
    {
        var isLeft = direction > 0;

        posX += isLeft ? 0.25f : -0.25f;
        posY += 1;

        Speed = isLeft ? 10 : -10;

        Player = player;
        ProjectileID = id;

        Position = new Vector3Model
        {
            X = posX, Y = posY, Z = posZ
        };

        StartTime = player.Room.Time;
        LifeTime = StartTime + lifeTime;
        _plane = Position.Z > 10 ? "Plane1" : "Plane0";

        //Magic Numbers 0.8f,, add to config
        PrjCollider = new BaseCollider(id, Position, 0.5f, 0.5f, _plane, player.Room, true);

        var prj = new LaunchItem_SyncEvent(player.GameObjectId.ToString(), StartTime, posX, posY, posZ, Speed, 0, LifeTime, ProjectileID, item.PrefabName);
        player.Room.SendSyncEvent(prj);
        //Logger.LogInformation("Created Synced Projectile with ID {args1} and lifetime {args2} at position ({args3}, {args4}, {args5})", ProjectileID, LifeTime, Position.X, Position.Y, Position.Z);
    }

    public override void Update()
    {
        base.Update();

        // The magic number here is the default game tickrate. This will be changed in a future commit
        Position.X += Speed * 0.015625f;
        PrjCollider.Position.x = Position.X;

        var Collisions = PrjCollider.IsColliding();

        if (Collisions.Length > 0)
            foreach (var collision in Collisions)
                ProjectileHit(collision.ToString());

        if (LifeTime <= Player.Room.Time)
            ProjectileHit("-1");
    }

    public void ProjectileHit(string hitGoID)
    {
        //Logger.LogInformation("Projectile with ID {args1} destroyed at position ({args2}, {args3}, {args4})", ProjectileID, Position.X, Position.Y, Position.Z);
        var hit = new ProjectileHit_SyncEvent(new SyncEvent(Player.GameObjectId.ToString(), SyncEvent.EventType.ProjectileHit, Player.Room.Time));

        hit.EventDataList.Add(ProjectileID);
        hit.EventDataList.Add(hitGoID);
        hit.EventDataList.Add(0);
        hit.EventDataList.Add(Position.X);
        hit.EventDataList.Add(Position.Y);

        Player.Room.SendSyncEvent(hit);
        Player.Room.Projectiles.Remove(ProjectileID);
    }
}
