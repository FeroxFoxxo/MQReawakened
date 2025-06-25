using A2m.Server;
using Discord;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;
using System.Collections.Specialized;
using System.Text.Json;
using WorldGraphDefines;

namespace Server.Reawakened.Rooms.Services;

public class WorldHandler(EventSink sink, ServerRConfig config, WorldGraph worldGraph,
    TimerThread timerThread, IServiceProvider services, ILogger<WorldHandler> logger,
    CharacterHandler handler) : IService
{
    private readonly Dictionary<int, Level> _levels = [];
    private readonly object Lock = new();

    public Dictionary<string, Type> EntityComponents { get; private set; } = [];
    public Dictionary<string, Type> ProcessableComponents { get; private set; } = [];
    public Dictionary<string, Dictionary<string, OrderedDictionary>> PrefabOverrides { get; private set; } = [];

    public ServerRConfig Config => config;

    public void Initialize() => sink.WorldLoad += LoadRooms;

    private void LoadRooms()
    {
        InternalDirectory.OverwriteDirectory(config.LevelDataSaveDirectory);

        foreach (var room in _levels
                     .Where(level => level.Key > 0)
                     .SelectMany(level => level.Value.Rooms))
            room.Value.DumpPlayersToLobby(this);

        _levels.Clear();

        GetClassComponents();
    }

    private void GetClassComponents()
    {
        EntityComponents = typeof(BaseComponent).Assembly.GetServices<BaseComponent>()
            .Where(t => t.BaseType != null)
            .Where(t => t.BaseType.GenericTypeArguments.Length > 0)
            .Select(t => new Tuple<string, Type>(t.BaseType.GenericTypeArguments.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name))?.Name, t))
            .Where(t => !string.IsNullOrEmpty(t.Item1))
            .ToDictionary(t => t.Item1, t => t.Item2);

        ProcessableComponents = typeof(DataComponentAccessor).Assembly.GetServices<DataComponentAccessor>()
            .ToDictionary(x => x.Name, x => x);

        var internalProcessableComponents = typeof(DataComponentAccessorMQR).Assembly.GetServices<DataComponentAccessorMQR>()
            .ToDictionary(x => x.Name, x => x);

        foreach (var internalProcessable in internalProcessableComponents)
        {
            var dataComp = Activator.CreateInstance(internalProcessable.Value) as DataComponentAccessorMQR;

            if (!ProcessableComponents.ContainsKey(dataComp.OverrideName))
            {
                logger.LogError("Unknown class to override for: {Class}!", dataComp.OverrideName);
                continue;
            }

            if (!EntityComponents.TryGetValue(internalProcessable.Key, out var entityComp))
            {
                logger.LogError("Unknown entity class for: {Class}!", internalProcessable.Key);
                continue;
            }

            ProcessableComponents.Remove(dataComp.OverrideName);
            ProcessableComponents.Add(dataComp.OverrideName, internalProcessable.Value);

            EntityComponents.Remove(internalProcessable.Key);
            EntityComponents.Add(dataComp.OverrideName, entityComp);
        }
    }

    public LevelInfo GetLevelInfo(int levelId)
    {
        if (levelId is not -1 and not 0)
            try
            {
                var levelInfo = worldGraph!.GetInfoLevel(levelId);

                if (string.IsNullOrEmpty(levelInfo.Name))
                    levelInfo = worldGraph!.GetInfoLevel(worldGraph.DefaultLevel);

                return string.IsNullOrEmpty(levelInfo.Name)
                    ? throw new MissingFieldException($"Room '{levelId}' does not have a valid name!")
                    : levelInfo;
            }
            catch (NullReferenceException)
            {
                if (_levels.Count == 0)
                    logger.LogCritical(
                        "Could not find any rooms! Are you sure you have your cache set up correctly?");
                else
                    logger.LogError("Could not find the required room! Are you sure your caches contain this?");
            }

        var name = levelId switch
        {
            -1 => "Disconnected",
            0 => "Lobby",
            _ => throw new ArgumentOutOfRangeException(nameof(levelId), levelId, null)
        };

        return new LevelInfo(name, name, name, levelId,
            0, 0, LevelType.Unknown, TribeType._Invalid);
    }

    public Room GetRoomFromLevelId(int levelId, Player player)
    {
        lock (Lock)
        {
            if (!_levels.ContainsKey(levelId))
                _levels.Add(levelId, new Level(GetLevelInfo(levelId)));
        }

        var level = _levels[levelId];

        Room room = null;

        lock (level.Lock)
        {
            if (level.Rooms.Count > 0)
            {
                if (level.LevelInfo.IsATrailLevel())
                {
                    if (player.TempData.Group != null)
                    {
                        var playerMembers = player.TempData.Group.GetMembers();

                        var trailRoom = level.Rooms.Values.FirstOrDefault(r =>
                            r.GetPlayers().Any(c => playerMembers.Contains(c))
                        );

                        if (trailRoom != null)
                            return trailRoom;
                    }
                }
                else
                {
                    return level.Rooms.Values.FirstOrDefault();
                }
            }

            var roomId = level.Rooms.Keys.Count > 0 ? level.Rooms.Keys.Max() + 1 : 1;

            room = new Room(roomId, level, services, timerThread, config);

            level.Rooms.Add(roomId, room);
        }

        return room;
    }

    public List<string> GetSurroundingLevels(LevelInfo levelInfo)
    {
        var nodes = worldGraph.GetLevelWorldGraphNodes(levelInfo.LevelId);

        return nodes == null
            ? []
            : [.. nodes
            .Where(x => x.ToLevelID != x.LevelID)
            .Select(x => worldGraph.GetInfoLevel(x.ToLevelID).Name)
            .Distinct()];
    }

    public void UsePortal(Player player, int levelId, int portalId, string defaultSpawnId = "")
    {
        var newLevelId = worldGraph.GetLevelFromPortal(levelId, portalId);

        if (newLevelId <= 0)
        {
            logger.LogError("Could not find level for portal {PortalId} in room {RoomId}", portalId, levelId);
            return;
        }

        var node = worldGraph.GetDestinationNodeFromPortal(levelId, portalId);

        string spawnId;

        if (node != null)
        {
            spawnId = node.ToSpawnID.ToString();
            logger.LogDebug("Node found! Portal ID: '{Portal}'. Spawn ID: '{Spawn}'.", node.PortalID, node.ToSpawnID);
        }
        else
        {
            spawnId = defaultSpawnId;
            logger.LogError("Could not find node for '{Old}' -> '{New}' for portal {PortalId}.", levelId, newLevelId, portalId);
        }

        if (levelId == newLevelId && player.Character.SpawnPointId == spawnId)
        {
            logger.LogError("Attempt made to teleport to the same portal! Skipping...");
            return;
        }

        var levelInfo = worldGraph.GetInfoLevel(newLevelId);

        logger.LogInformation(
            "Teleporting {CharacterName} ({CharacterId}) to {LevelName} ({LevelId}) " +
            "using portal {PortalId}", player.Character.CharacterName,
            player.Character.Id, levelInfo.InGameName, levelInfo.LevelId, portalId
        );

        ChangePlayerRoom(player, newLevelId, spawnId);
    }

    public bool ChangePlayerRoom(Player player, int levelId, string spawnId = "") =>
        _ = TryChangePlayerRoom(player, levelId, spawnId);

    public bool TryChangePlayerRoom(Player player, int levelId, string spawnId = "")
    {
        var levelInfo = worldGraph.GetInfoLevel(levelId);

        if (string.IsNullOrEmpty(levelInfo.Name) || !config.LoadedAssets.Contains(levelInfo.Name))
        {
            logger.LogError("Player: {player} specified an invalid level!", player);
            return false;
        }

        player.Character.Write.SpawnPointId = spawnId;

        if (player.Character.LevelId == levelInfo.LevelId)
        {
            player.Room.SetPlayerPosition(player.Character);
            player.TeleportPlayer(player.Character.SpawnPositionX, player.Character.SpawnPositionY, player.Character.SpawnOnBackPlane);
        }
        else
        {
            player.Character.Write.LevelId = levelInfo.LevelId;
            player.SendLevelChange(this);
        }

        handler.Update(player.Character.Write);

        return true;
    }

    public Dictionary<string, OrderedDictionary> GetPrefabOverloads(AssetBundleRConfig rConfig, string prefabName)
    {
        if (PrefabOverrides.TryGetValue(prefabName, out var foundValue))
            return foundValue;

        var prefabPath = Path.Combine(rConfig.ScriptsConfigDirectory, $"{prefabName.ToLower()}.json");
        Dictionary<string, OrderedDictionary> prefabOverrides = null;

        if (File.Exists(prefabPath))
        {
            var prefabText = File.ReadAllText(prefabPath);
            prefabOverrides = JsonSerializer.Deserialize<Dictionary<string, OrderedDictionary>>(prefabText);
        }

        PrefabOverrides.Add(prefabName, prefabOverrides);

        return prefabOverrides;
    }
}
