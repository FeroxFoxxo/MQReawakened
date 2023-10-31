using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Groups;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Players;

public class Player(UserInfo userInfo, NetState state, PlayerHandler playerHandler) : INetStateData
{
    public NetState NetState => state;
    public UserInfo UserInfo => userInfo;
    public PlayerHandler PlayerHandler => playerHandler;

    public TemporaryDataModel TempData { get; set; } = new TemporaryDataModel();

    public int UserId => userInfo.UserId;
    public string CharacterName => Character.Data.CharacterName;

    public GroupModel Group { get; set; }

    public CharacterModel Character { get; set; }
    public bool FirstLogin { get; set; } = true;

    public Room Room { get; set; }
    public int GameObjectId { get; set; }

    public bool OnGround { get; set; }
    public int Direction { get; set; }
    public Vector3Model Position { get; set; } = new Vector3Model();
    public Vector3Model Velocity { get; set; } = new Vector3Model();
    public bool Invincible { get; set; } = false;

    public long CurrentPing { get; set; } = GetTime.GetCurrentUnixMilliseconds();

    public void RemovedState(NetState state, NetStateHandler handler,
        Microsoft.Extensions.Logging.ILogger logger) => Remove(logger);

    public void Remove(Microsoft.Extensions.Logging.ILogger logger)
    {
        lock(playerHandler.Lock)
            playerHandler.RemovePlayer(this);

        this.RemoveFromGroup();

        if (Character != null)
        {
            lock (playerHandler.Lock)
            {
                foreach (var player in playerHandler.GetPlayersByFriend(UserId))
                    player.SendXt("fz", Character.Data.CharacterName);
            }

            if (TempData.TradeModel != null)
            {
                var tradingPlayer = TempData.TradeModel.TradingPlayer;
                tradingPlayer.TempData.TradeModel = null;
                tradingPlayer.SendXt("tc", Character.Data.CharacterName);
            }

            Character = null;
        }

        if (Room != null)
        {
            if (!Room.LevelInfo.IsValid())
                return;

            var roomName = Room.LevelInfo.Name;

            if (!string.IsNullOrEmpty(roomName))
                logger.LogDebug("Dumped player with ID '{User}' from room '{Room}'", UserId, roomName);
        }

        this.DumpToLobby();
    }
}
