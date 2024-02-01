using Server.Reawakened.Configs;
using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Players.Helpers;

public class DatabaseContainer(WorldHandler worldHandler, CharacterHandler characterHandler,
    QuestCatalog quests, InternalObjective objCatalog, ItemCatalog itemCatalog,
    InternalAchievement internalAchievement, EventPrefabs eventPrefabs, ServerRConfig config)
{
    public WorldHandler WorldHandler => worldHandler;
    public CharacterHandler CharacterHandler => characterHandler;
    public QuestCatalog Quests => quests;
    public InternalObjective Objectives => objCatalog;
    public ItemCatalog ItemCatalog => itemCatalog;
    public InternalAchievement InternalAchievement => internalAchievement;
    public EventPrefabs EventPrefabs => eventPrefabs;
    public ServerRConfig ServerRConfig => config;

    public UserInfoHandler UserInfoHandler;

    public object Lock { get; } = new object();

    private readonly List<Player> _playerList = [];

    public void AddPlayer(Player player)
    {
        lock (Lock)
            _playerList.Add(player);
    }

    public void RemovePlayer(Player player)
    {
        lock (Lock)
            _playerList.Remove(player);
    }

    public List<Player> GetAllPlayers()
    {
        lock (Lock)
            return [.. _playerList];
    }

    public IEnumerable<Player> GetPlayersByFriend(int friendId)
    {
        lock (Lock)
            return _playerList.ToList().Where(p => p.Character.Data.Friends.Contains(friendId));
    }

    public IEnumerable<Player> GetPlayersByCharacterId(int characterId)
    {
        lock (Lock)
            return _playerList.ToList().Where(p => p.CharacterId == characterId);
    }

    public IEnumerable<Player> GetPlayersByUserId(int playerId)
    {
        lock (Lock)
            return _playerList.ToList().Where(p => p.UserId == playerId);
    }

    public bool AnyPlayersByUserId(int playerId)
    {
        lock (Lock)
            return _playerList.ToList().Any(p => p.UserId == playerId);
    }

    public Player GetPlayerByName(string characterName)
    {
        lock (Lock)
            return _playerList.ToList().Find(p => p.CharacterName == characterName);
    }
}
