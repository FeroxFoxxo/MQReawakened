using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Models.Planes;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using System.Text;
using WorldGraphDefines;

namespace Protocols.External._s__Synchronizer;

public class State : ExternalProtocol
{
    public override string ProtocolName => "ss";

    public SyncEventManager SyncEventManager { get; set; }
    public ILogger<State> Logger { get; set; }
    public ServerStaticConfig ServerConfig { get; set; }
    public LevelHandler LevelHandler { get; set; }
    public NetworkLogger NetworkLogger { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var level = player.GetCurrentLevel(LevelHandler);

        if (level == null)
            return;

        var syncEvent = SyncEventManager.DecodeEvent(message[5].Split('&'));

        if (ServerConfig.LogSyncState)
            Logger.LogInformation("Found state: {State}", syncEvent.Type);

        var entityId = int.Parse(syncEvent.TargetID);

        if (entityId == player.PlayerId)
        {
            switch (syncEvent.Type)
            {
                case SyncEvent.EventType.ChargeAttack:
                    var chargeAttackEvent = new ChargeAttack_SyncEvent(syncEvent);

                    var startEvent = new ChargeAttackStart_SyncEvent(entityId.ToString(), chargeAttackEvent.TriggerTime,
                        chargeAttackEvent.PosX, chargeAttackEvent.PosY, chargeAttackEvent.SpeedX,
                        chargeAttackEvent.SpeedY,
                        chargeAttackEvent.ItemId, chargeAttackEvent.ZoneId);

                    NetState.SendSyncEventToPlayer(startEvent);

                    Logger.LogWarning("Collision system not yet written and implemented for {Type}.",
                        chargeAttackEvent.Type);
                    break;
                case SyncEvent.EventType.NotifyCollision:
                    var notifyCollisionEvent = new NotifyCollision_SyncEvent(syncEvent);
                    var collisionTarget = int.Parse(notifyCollisionEvent.CollisionTarget);

                    if (level.LevelEntities.Entities.TryGetValue(collisionTarget, out var entities))
                        foreach (var entity in entities)
                            entity.NotifyCollision(notifyCollisionEvent, NetState);
                    else
                        Logger.LogWarning("Unhandled collision From {TargetId} ", collisionTarget);

                    break;
                case SyncEvent.EventType.PhysicBasic:
                    var physicsBasicEvent = new PhysicBasic_SyncEvent(syncEvent);

                    player.Position = new Vector3Model
                    {
                        X = physicsBasicEvent.PositionX,
                        Y = physicsBasicEvent.PositionY,
                        Z = physicsBasicEvent.PositionZ
                    };

                    player.Velocity = new Vector3Model
                    {
                        X = physicsBasicEvent.VelocityX,
                        Y = physicsBasicEvent.VelocityY,
                        Z = physicsBasicEvent.VelocityZ
                    };

                    player.OnGround = physicsBasicEvent.OnGround;
                    break;
                case SyncEvent.EventType.Direction:
                    var directionEvent = new Direction_SyncEvent(syncEvent);
                    player.Direction = directionEvent.Direction;
                    break;
            }

            player.CurrentLevel.SendSyncEvent(syncEvent, player);
        }
        else if (level.LevelEntities.Entities.TryGetValue(entityId, out var entities))
        {
            foreach (var entity in entities)
                entity.RunSyncedEvent(syncEvent, NetState);
        }
        else
        {
            switch (syncEvent.Type)
            {
                default:
                    var entity = level.LevelPlanes.Planes.Values.SelectMany(x => x.GameObjects)
                        .FirstOrDefault(x => x.Key == entityId);

                    var components = new List<string>();

                    if (entity.Value != null)
                        foreach (var component in entity.Value.ObjectInfo.Components)
                        {
                            LevelHandler.ProcessableData.TryGetValue(component.Key, out var mqType);

                            if (mqType != null)
                                components.Add(mqType.Name);
                        }

                    if (components.Count == 0)
                        components.Add("UNKNOWN");

                    var names = string.Join(", ", components);
                    
                    TraceSyncEventError(entityId, syncEvent, level.LevelInfo, names);

                    break;
            }
        }
    }

    public void TraceSyncEventError(int entityId, SyncEvent syncEvent, LevelInfo levelInfo, string names)
    {
        var builder = new StringBuilder()
            .AppendLine($"# {DateTime.UtcNow} @ Sync Entity: {entityId} ({names})")
            .AppendLine($"Level: {levelInfo.LevelId} ({levelInfo.InGameName})")
            .AppendLine($"Sync Event Type: {syncEvent.Type}")
            .AppendLine()
            .AppendLine(syncEvent.EncodeData());

        NetworkLogger.WriteToFile<SyncEvent>("event-errors.log", builder, LoggerType.Warning);
    }
}
