using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Models.LevelData;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._s__Synchronizer;

public class State : ExternalProtocol
{
    public override string ProtocolName => "ss";

    public SyncEventManager SyncEventManager { get; set; }
    public ILogger<State> Logger { get; set; }
    public ServerConfig ServerConfig { get; set; }
    public LevelHandler LevelHandler { get; set; }

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
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (syncEvent.Type)
            {
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
        else if (level.LevelEntityHandler.Entities.TryGetValue(entityId, out var entities))
        {
            foreach (var entity in entities)
                entity.RunSyncedEvent(syncEvent, NetState);
        }
        else
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (syncEvent.Type)
            {
                default:
                    var entity = level.LevelPlaneHandler.Planes.Values.SelectMany(x => x.GameObjects)
                        .FirstOrDefault(x => x.Key == entityId);

                    var components = new List<string>();

                    if (entity.Value != null)
                        foreach (var component in entity.Value.ObjectInfo.Components)
                        {
                            level.LevelHandler.ProcessableData.TryGetValue(component.Key, out var mqType);

                            if (mqType != null)
                                components.Add(mqType.Name);
                        }

                    if (components.Count == 0)
                        components.Add("UNKNOWN");

                    Logger.LogWarning("Unhandled Sync Event For {EntityId} ({EntityName}).", entityId, string.Join(", ", components));

                    break;
            }
        }
    }
}
