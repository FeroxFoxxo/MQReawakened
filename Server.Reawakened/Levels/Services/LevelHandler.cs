using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.XMLs.Bundles;
using System.Text.Json;
using System.Xml;
using WorldGraphDefines;

namespace Server.Reawakened.Levels.Services;

public class LevelHandler : IService
{
    private readonly ServerStaticConfig _config;
    private readonly Dictionary<int, List<Level>> _levels;
    private readonly Dictionary<int, LevelInfo> _levelInfos;
    private readonly ILogger<LevelHandler> _logger;
    private readonly ReflectionUtils _reflection;
    private readonly TimerThread _timerThread;
    private readonly IServiceProvider _services;
    private readonly EventSink _sink;
    private readonly WorldGraph _worldGraph;

    public Dictionary<string, Type> ProcessableData;

    public LevelHandler(EventSink sink, ServerStaticConfig config, WorldGraph worldGraph,
        ReflectionUtils reflection, TimerThread timerThread, IServiceProvider services,
        ILogger<LevelHandler> logger)
    {
        _sink = sink;
        _config = config;
        _worldGraph = worldGraph;
        _reflection = reflection;
        _timerThread = timerThread;
        _services = services;
        _logger = logger;

        _levelInfos = new Dictionary<int, LevelInfo>();
        _levels = new Dictionary<int, List<Level>>();
    }

    public void Initialize() => _sink.WorldLoad += LoadLevels;

    private void LoadLevels()
    {
        GetDirectory.OverwriteDirectory(_config.LevelDataSaveDirectory);

        ProcessableData = typeof(DataComponentAccessor).Assembly.GetServices<DataComponentAccessor>()
            .ToDictionary(x => x.Name, x => x);

        foreach (var levelList in _levels.Where(level => level.Key != -1))
        foreach (var level in levelList.Value)
            level.DumpPlayersToLobby();

        _levels.Clear();
        _levelInfos.Clear();
    }

    public LevelInfo GetLevelInfo(int levelId)
    {
        if (levelId is not -1 and not 0)
            try
            {
                var levelInfo = _worldGraph!.GetInfoLevel(levelId);

                return string.IsNullOrEmpty(levelInfo.Name)
                    ? throw new MissingFieldException($"Level '{levelId}' does not have a valid name!")
                    : levelInfo;
            }
            catch (NullReferenceException)
            {
                if (_levels.Count == 0)
                    _logger.LogCritical(
                        "Could not find any levels! Are you sure you have your cache set up correctly?");
                else
                    _logger.LogError("Could not find the required level! Are you sure your caches contain this?");
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

    public Level GetLevelFromId(int levelId)
    {
        if (!_levelInfos.ContainsKey(levelId))
            _levelInfos.Add(levelId, GetLevelInfo(levelId));

        var levelInfo = _levelInfos[levelId];

        if (!_levels.ContainsKey(levelId))
            _levels.Add(levelId, new List<Level>());

        if (_levels[levelId].Count > 0)
        {
            if (levelInfo.IsATrailLevel())
            {
                // Check if friends are in level.
            }
            else
            {
                return _levels[levelId].FirstOrDefault();
            }
        }

        var levelPlanes = new LevelPlanes();

        if (levelInfo.Type != LevelType.Unknown)
        {
            var levelInfoPath = Path.Join(_config.LevelSaveDirectory, $"{levelInfo.Name}.xml");
            var levelDataPath = Path.Join(_config.LevelDataSaveDirectory, $"{levelInfo.Name}.json");

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(levelInfoPath);
            levelPlanes.LoadXmlDocument(xmlDocument);

            File.WriteAllText(levelDataPath,
                JsonSerializer.Serialize(levelPlanes, new JsonSerializerOptions { WriteIndented = true }));
        }

        var level = new Level(levelInfo, levelPlanes, _config, this,
            _reflection, _timerThread, _services, _logger);

        _levels[levelId].Add(level);

        return level;
    }

    public void RemoveLevel(Level level) => _levels[level.LevelInfo.LevelId].Remove(level);
}
