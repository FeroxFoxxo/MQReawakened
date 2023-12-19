using A2m.Server;
using agsXMPP;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Entity;
public class ProjectileEntity : Component<ProjectileController>
{
    public Vector3Model Position;
    public float Speed, LifeTime, StartTime;
    public Player Player;
    public int ProjectileID = 0;
    public UnityEngine.Vector2 CurrentPosition;
    private readonly string _plane;
    public ILogger<ProjectileEntity> Logger { get; set; }

    public ProjectileEntity(Player player, int id, float posX, float posY, float posZ, int direction, float lifeTime, ItemDescription item)
    {
        var isLeft = direction > 0;
        posX += isLeft ? 0.25f : -0.25f;
        posY += 1;
        Speed = isLeft ? 10 : -10;

        Player = player;
        ProjectileID = id;
        Position = new Vector3Model();
        Position.X = posX; Position.Y = posY; Position.Z = posZ;

        StartTime = player.Room.Time;
        LifeTime = StartTime + lifeTime;
        CurrentPosition = new UnityEngine.Vector2(posX, posY);
        _plane = Position.Z > 10 ? "Plane1" : "Plane0";

        var prj = new LaunchItem_SyncEvent(player.GameObjectId.ToString(), StartTime, posX, posY, posZ, Speed, 0, LifeTime, ProjectileID, item.PrefabName);
        player.Room.SendSyncEvent(prj);
        //Logger.LogInformation("Created Synced Projectile with ID {args1} and lifetime {args2} at position ({args3}, {args4}, {args5})", ProjectileID, LifeTime, Position.X, Position.Y, Position.Z);
    }

    public override void Update()
    {
        base.Update();
        Position.X += Speed * 0.015625f;

        //This is extremely bad code, but this will be updated properly as soon as the ProjectileHit_SyncEvent crash is fixed.
        foreach (var obj in
                 Player.Room.Planes[_plane].GameObjects.Values
                     .Where(obj => Vector3Model.Distance(Position, obj.ObjectInfo.Position) <= 1.5f)
                )
        {
            var isLeft = Speed > 0;

            if (isLeft)
            {
                if (obj.ObjectInfo.Position.X < Position.X)
                    continue;
            }
            else
            {
                if (obj.ObjectInfo.Position.X > Position.X)
                    continue;
            }

            var objectId = obj.ObjectInfo.ObjectId;

            if (Player.Room.Entities.TryGetValue(objectId, out var entityComponents) && Player.Room.Time >= StartTime+0.2)
                foreach (var component in entityComponents)
                    if (component is BreakableEventControllerComp breakableObjEntity)
                    {
                        breakableObjEntity.Destroy(Player);
                        ProjectileHit("-1");
                        //ProjectileHit(obj.ObjectInfo.ObjectId.ToString());
                    }

                    else if (component is InterObjStatusComp enemyEntity && component.Id > 0)
                    {
                        enemyEntity.SendDamageEvent(Player);
                        ProjectileHit("-1");
                        //ProjectileHit(obj.ObjectInfo.ObjectId.ToString());
                    }
        }

        if (LifeTime <= Player.Room.Time)
            ProjectileHit("-1");
    }

    public void ProjectileHit(string hitGoID)
    {
        //Logger.LogInformation("Projectile with ID {args1} destroyed at position ({args2}, {args3}, {args4})", ProjectileID, Position.X, Position.Y, Position.Z);
        var hit = new ProjectileHit_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time, ProjectileID, hitGoID, false);
        Player.Room.SendSyncEvent(hit);
        Player.Room.Projectiles.Remove(ProjectileID);
    }
}
