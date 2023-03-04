using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Bundles;
using WorldGraphDefines;

namespace Server.Reawakened.Rooms.Services;

public class WorldHandler : IService
{
    private readonly ServerRConfig _config;

    private readonly ILogger<WorldHandler> _handlerLogger;

    private readonly ILogger<Room> _roomLogger;
    private readonly IServiceProvider _services;
    private readonly EventSink _sink;
    private readonly TimerThread _timerThread;
    private readonly WorldGraph _worldGraph;
    private readonly Dictionary<int, Level> _levels;

    public WorldHandler(EventSink sink, ServerRConfig config, WorldGraph worldGraph,
        TimerThread timerThread, IServiceProvider services, ILogger<WorldHandler> handlerLogger,
        ILogger<Room> roomLogger)
    {
        _sink = sink;
        _config = config;
        _worldGraph = worldGraph;
        _timerThread = timerThread;
        _services = services;
        _handlerLogger = handlerLogger;
        _roomLogger = roomLogger;

        _levels = new Dictionary<int, Level>();
    }

    public void Initialize() => _sink.WorldLoad += LoadRooms;

    private void LoadRooms()
    {
        InternalDirectory.OverwriteDirectory(_config.LevelDataSaveDirectory);

        foreach (var room in _levels
                     .Where(level => level.Key > 0)
                     .SelectMany(level => level.Value.Rooms))
            room.Value.DumpPlayersToLobby();

        _levels.Clear();
    }

    public LevelInfo GetLevelInfo(int levelId)
    {
        if (levelId is not -1 and not 0)
            try
            {
                var levelInfo = _worldGraph!.GetInfoLevel(levelId);

                return string.IsNullOrEmpty(levelInfo.Name)
                    ? throw new MissingFieldException($"Room '{levelId}' does not have a valid name!")
                    : levelInfo;
            }
            catch (NullReferenceException)
            {
                if (_levels.Count == 0)
                    _handlerLogger.LogCritical(
                        "Could not find any rooms! Are you sure you have your cache set up correctly?");
                else
                    _handlerLogger.LogError("Could not find the required room! Are you sure your caches contain this?");
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

    public Room GetRoomFromLevelId(int levelId, NetState state)
    {
        if (!_levels.ContainsKey(levelId))
            _levels.Add(levelId, new Level(GetLevelInfo(levelId)));

        var level = _levels[levelId];

        if (level.Rooms.Count > 0)
        {
            if (level.LevelInfo.IsATrailLevel())
            {
                var player = state.Get<Player>();

                if (player.Group != null)
                {
                    var trailRoom = level.Rooms.Values.FirstOrDefault(r =>
                        r.Clients.Any(c => player.Group.GroupMembers.Contains(c.Value))
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

        var roomId = 1;

        while (level.Rooms.ContainsKey(roomId))
            roomId++;

        var room = new Room(roomId, level, _config, _timerThread, _services, _roomLogger, this);

        level.Rooms.Add(roomId, room);

        return room;
    }
}
