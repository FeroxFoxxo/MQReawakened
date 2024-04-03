using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Models;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;

namespace Server.Reawakened.Players;

public class Player(Account account, UserInfo userInfo, NetState state, WorldHandler worldHandler, PlayerContainer playerContainer, CharacterHandler characterHandler) : INetStateData
{
    public Account Account => account;
    public NetState NetState => state;
    public UserInfo UserInfo => userInfo;
    public PlayerContainer PlayerContainer => playerContainer;
    public CharacterHandler CharacterHandler => characterHandler;

    public TemporaryDataModel TempData { get; set; } = new TemporaryDataModel();
    public CharacterModel Character { get; set; }
    public Room Room { get; set; }

    public int UserId => userInfo.Id;
    public int CharacterId => Character != null ? Character.Id : -1;
    public string CharacterName => Character != null ? Character.Data.CharacterName : string.Empty;
    public string GameObjectId => TempData.GameObjectId;

    public bool FirstLogin { get; set; } = true;
    public long CurrentPing { get; set; } = GetTime.GetCurrentUnixMilliseconds();

    public void RemovedState(NetState state, NetStateHandler handler,
        Microsoft.Extensions.Logging.ILogger logger) => Remove(logger);

    public void Remove(Microsoft.Extensions.Logging.ILogger logger)
    {
        lock (PlayerContainer.Lock)
            playerContainer.RemovePlayer(this);

        this.RemoveFromGroup();

        if (Character != null)
        {
            lock (PlayerContainer.Lock)
            {
                foreach (var player in playerContainer.GetPlayersByFriend(CharacterId))
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
                logger.LogDebug("Dumped _player with ID '{User}' from room '{Room}'", UserId, roomName);
        }

        this.DumpToLobby(worldHandler);

        try
        {
            NetState.Dispose();
        }
        catch (Exception) { }
    }
}
