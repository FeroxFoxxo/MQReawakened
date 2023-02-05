using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Models.LevelData;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;

namespace Protocols.External._s__Synchronizer;

public class State : ExternalProtocol
{
    public SyncEventManager SyncEventManager { get; set; }
    public ILogger<State> Logger { get; set; }
    public ServerConfig ServerConfig { get; set; }

    public override string ProtocolName => "ss";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

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

            player.CurrentLevel.RelayClientSync(syncEvent, player);
        }
    }
}
