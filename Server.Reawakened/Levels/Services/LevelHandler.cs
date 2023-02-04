using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Models;
using Server.Reawakened.Levels.Models.LevelData;
using Server.Reawakened.XMLs.Bundles;
using System.Text.Json;
using System.Xml;
using WorldGraphDefines;

namespace Server.Reawakened.Levels.Services;

public class LevelHandler : IService
{
    private readonly ServerConfig _config;
    private readonly Dictionary<int, Level> _levels;
    private readonly ILogger<LevelHandler> _logger;
    private readonly EventSink _sink;
    private readonly WorldGraph _worldGraph;

    public LevelHandler(EventSink sink, ServerConfig config, WorldGraph worldGraph, ILogger<LevelHandler> logger)
    {
        _sink = sink;
        _config = config;
        _worldGraph = worldGraph;
        _logger = logger;
        _levels = new Dictionary<int, Level>();
    }

    public void Initialize() => _sink.WorldLoad += LoadLevels;

    private void LoadLevels()
    {
        GetDirectory.OverwriteDirectory(_config.LevelDataSaveDirectory);

        foreach (var level in _levels.Values.Where(level => level.LevelInfo.LevelId != -1))
            level.DumpPlayersToLobby();

        _levels.Clear();
    }

    public Level GetLevelFromId(int levelId)
    {
        if (_levels.TryGetValue(levelId, out var value))
            return value;

        LevelInfo levelInfo = null;
        var levelData = new LevelDataModel();

        if (levelId is -1 or 0)
        {
            var name = levelId switch
            {
                -1 => "Disconnected",
                0 => "Lobby",
                _ => throw new ArgumentOutOfRangeException(nameof(levelId), levelId, null)
            };

            levelInfo = new LevelInfo(name, name, name, levelId,
                0, 0, LevelType.Unknown, TribeType._Invalid);
        }
        else
        {
            try
            {
                levelInfo = _worldGraph!.GetInfoLevel(levelId);

                var levelInfoPath = Path.Join(_config.LevelSaveDirectory, $"{levelInfo.Name}.xml");
                var levelDataPath = Path.Join(_config.LevelDataSaveDirectory, $"{levelInfo.Name}.json");

                var xmlDocument = new XmlDocument();
                xmlDocument.Load(levelInfoPath);
                levelData.LoadXmlDocument(xmlDocument);
                
                File.WriteAllText(levelDataPath, JsonSerializer.Serialize(levelData, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (NullReferenceException)
            {
                if (_levels.Count == 0)
                    _logger.LogCritical(
                        "Could not find any levels! Are you sure you have your cache set up correctly?");
                else
                    _logger.LogError("Could not find the required level! Are you sure your caches contain this?");
            }
        }

        var level = new Level(levelInfo, levelData, _config, this, _worldGraph, _logger);

        _levels.Add(levelId, level);

        return level;
    }
}
