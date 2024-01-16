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
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Players;

public class Player(Account account, UserInfo userInfo, NetState state, DatabaseContainer databaseContainer) : INetStateData
{
    public Account Account => account;
    public NetState NetState => state;
    public UserInfo UserInfo => userInfo;
    public DatabaseContainer DatabaseContainer => databaseContainer;

    public TemporaryDataModel TempData { get; set; } = new TemporaryDataModel();
    public CharacterModel Character { get; set; }
    public Room Room { get; set; }

    public int UserId => userInfo.Id;
    public int CharacterId => Character != null ? Character.Id : -1;
    public string CharacterName => Character != null ? Character.Data.CharacterName : string.Empty;
    public int GameObjectId => TempData.GameObjectId;

    public bool FirstLogin { get; set; } = true;
    public long CurrentPing { get; set; } = GetTime.GetCurrentUnixMilliseconds();

    public void RemovedState(NetState state, NetStateHandler handler,
        Microsoft.Extensions.Logging.ILogger logger) => Remove(logger);

    public void Remove(Microsoft.Extensions.Logging.ILogger logger)
    {
        lock (databaseContainer.Lock)
            databaseContainer.RemovePlayer(this);

        this.RemoveFromGroup();

        if (Character != null)
        {
            lock (databaseContainer.Lock)
            {
                foreach (var player in databaseContainer.GetPlayersByFriend(CharacterId))
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
