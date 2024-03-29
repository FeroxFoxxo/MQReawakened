﻿using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Bundles;
using WorldGraphDefines;

namespace Server.Reawakened.Rooms.Services;

public class WorldHandler(EventSink sink, ServerRConfig config, WorldGraph worldGraph,
    TimerThread timerThread, IServiceProvider services, ILogger<WorldHandler> handlerLogger) : IService
{
    private readonly Dictionary<int, Level> _levels = [];
    private readonly object Lock = new();

    public void Initialize() => sink.WorldLoad += LoadRooms;

    private void LoadRooms()
    {
        InternalDirectory.OverwriteDirectory(config.LevelDataSaveDirectory);

        foreach (var room in _levels
                     .Where(level => level.Key > 0)
                     .SelectMany(level => level.Value.Rooms))
            room.Value.DumpPlayersToLobby(this);

        _levels.Clear();
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
                    handlerLogger.LogCritical(
                        "Could not find any rooms! Are you sure you have your cache set up correctly?");
                else
                    handlerLogger.LogError("Could not find the required room! Are you sure your caches contain this?");
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
                            r.Players.Any(c => playerMembers.Contains(c.Value))
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

            room = new Room(roomId, level, timerThread, services, config);

            level.Rooms.Add(roomId, room);
        }

        return room;
    }

    public List<string> GetSurroundingLevels(LevelInfo levelInfo)
    {
        var nodes = worldGraph.GetLevelWorldGraphNodes(levelInfo.LevelId);

        return nodes == null
            ? []
            : nodes
            .Where(x => x.ToLevelID != x.LevelID)
            .Select(x => worldGraph.GetInfoLevel(x.ToLevelID).Name)
            .Distinct()
            .ToList();
    }
}
