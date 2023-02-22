using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.XMLs.Bundles;
using WorldGraphDefines;

namespace Server.Reawakened.Rooms.Services;

public class WorldHandler : IService
{
    private readonly ServerStaticConfig _config;

    private readonly ILogger<WorldHandler> _handlerLogger;
    private readonly Dictionary<int, LevelInfo> _levelInfos;

    private readonly ReflectionUtils _reflection;
    private readonly ILogger<Room> _roomLogger;
    private readonly Dictionary<int, List<Room>> _rooms;
    private readonly IServiceProvider _services;
    private readonly EventSink _sink;
    private readonly TimerThread _timerThread;
    private readonly WorldGraph _worldGraph;
    private readonly FileLogger _fileLogger;

    public WorldHandler(EventSink sink, ServerStaticConfig config, WorldGraph worldGraph,
        ReflectionUtils reflection, TimerThread timerThread, IServiceProvider services,
        ILogger<WorldHandler> handlerLogger, ILogger<Room> roomLogger, FileLogger fileLogger)
    {
        _sink = sink;
        _config = config;
        _worldGraph = worldGraph;
        _reflection = reflection;
        _timerThread = timerThread;
        _services = services;
        _handlerLogger = handlerLogger;
        _roomLogger = roomLogger;
        _fileLogger = fileLogger;

        _levelInfos = new Dictionary<int, LevelInfo>();
        _rooms = new Dictionary<int, List<Room>>();
    }

    public void Initialize() => _sink.WorldLoad += LoadRooms;

    private void LoadRooms()
    {
        GetDirectory.OverwriteDirectory(_config.LevelDataSaveDirectory);

        foreach (var roomList in _rooms.Where(room => room.Key != -1))
        foreach (var room in roomList.Value)
            room.DumpPlayersToLobby();

        _rooms.Clear();
        _levelInfos.Clear();
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
                if (_rooms.Count == 0)
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

    public Room GetRoomFromLevelId(int levelId)
    {
        if (!_levelInfos.ContainsKey(levelId))
            _levelInfos.Add(levelId, GetLevelInfo(levelId));

        var levelInfo = _levelInfos[levelId];

        if (!_rooms.ContainsKey(levelId))
            _rooms.Add(levelId, new List<Room>());

        if (_rooms[levelId].Count > 0)
        {
            if (levelInfo.IsATrailLevel())
            {
                // Check if friends are in room.
            }
            else
            {
                return _rooms[levelId].FirstOrDefault();
            }
        }

        var room = new Room(levelInfo, _config, this,
            _reflection, _timerThread, _services, _roomLogger, _fileLogger);

        _rooms[levelId].Add(room);

        return room;
    }

    public void RemoveRoom(Room room) => _rooms[room.LevelInfo.LevelId].Remove(room);
}
