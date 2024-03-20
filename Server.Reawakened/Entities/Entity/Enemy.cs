using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity.Utils;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Entity;

public class Enemy
{

    public bool Init;

    public Room Room;
    public string Id;
    public Vector3 Position;
    public Rect DetectionRange;
    public EnemyCollider Hitbox;
    public string ParentPlane;
    public bool IsFromSpawner;
    public float MinBehaviorTime;

    public int Health;
    public int MaxHealth;
    public int Level;
    public int DeathXp;
    public string OnDeathTargetId;

    public BaseComponent Entity;
    public InterObjStatusComp Status;
    public EnemyControllerComp EnemyController;

    public GlobalProperties EnemyGlobalProps;
    public AIProcessData AiData;
    public AISyncEventHelper SyncBuilder;

}
