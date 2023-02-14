using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Worlds;
using Server.Reawakened.Players.Events;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;
using Web.Launcher.Models;
using Web.Launcher.Models.Current;

namespace Web.Launcher.Services;

public class StartGame : IService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ServerConsole _console;
    private readonly LauncherStaticConfig _lConfig;
    private readonly ILogger<StartGame> _logger;
    private readonly SettingsStaticConfig _sConfig;
    private readonly StartConfig _config;
    private readonly EventSink _sink;
    private readonly World _world;
    private readonly PlayerEventSink _playerEventSink;

    private string _directory;
    private bool _dirSet, _appStart;
    private Process _game;

    public PackageInformation CurrentVersion { get; private set; }

    public StartGame(EventSink sink, LauncherStaticConfig lConfig, SettingsStaticConfig sConfig,
        IHostApplicationLifetime appLifetime, ILogger<StartGame> logger, ServerConsole console, World world, StartConfig config, PlayerEventSink playerEventSink)
    {
        _sink = sink;
        _lConfig = lConfig;
        _sConfig = sConfig;
        _appLifetime = appLifetime;
        _logger = logger;
        _console = console;
        _world = world;
        _config = config;
        _playerEventSink = playerEventSink;

        _dirSet = false;
        _appStart = false;
    }

    public void Initialize()
    {
        if (_config.IsHeadless)
        {
            _logger.LogWarning("NOT RESTARTING: SERVER IS HEADLESS");
        }
        else
        {
            _appLifetime.ApplicationStarted.Register(AppStarted);
            _sink.WorldLoad += GetGameInformation;
            _sink.Shutdown += StopGame;
            _playerEventSink.PlayerRefreshed += AskIfRestart;
        }
    }

    private void StopGame() => _game?.CloseMainWindow();

    private void AppStarted()
    {
        _appStart = true;
        RunGame();
    }

    private void GetGameInformation()
    {
        _console.AddCommand(new ConsoleCommand("runLauncher",
            "Runs the launcher and hooks it into the current process.",
            _ => LaunchGame()));

        _logger.LogInformation("Getting Game Executable");

        try
        {
            _config.GameSettingsFile = SetFileValue.SetIfNotNull(_config.GameSettingsFile, "Get Settings File",
                "Settings File (*.txt)\0*.txt\0");

            _sConfig.SetSettings(_config);
        }
        catch
        {
            // ignored
        }

        while (true)
        {
            if (string.IsNullOrEmpty(_config.GameSettingsFile) || !_config.GameSettingsFile.EndsWith("settings.txt"))
            {
                _logger.LogError("Please enter the absolute file path for your game's 'settings.txt' file.");
                _config.GameSettingsFile = Console.ReadLine();
                continue;
            }

            _directory = Path.GetDirectoryName(_config.GameSettingsFile);

            if (string.IsNullOrEmpty(_directory))
                continue;

            CurrentVersion =
                JsonSerializer.Deserialize<PackageInformation>(File.ReadAllText(Path.Join(_directory, "current.txt")));

            break;
        }

        _logger.LogDebug("Got launcher directory: {Directory}", Path.GetDirectoryName(_config.GameSettingsFile));

        var lastUpdate = DateTime.ParseExact(CurrentVersion.game.lastUpdate, _lConfig.TimeFilter,
            CultureInfo.InvariantCulture);
        var lastOldClientUpdate = DateTime.ParseExact(_lConfig.OldClientLastUpdate, _lConfig.TimeFilter,
            CultureInfo.InvariantCulture);

        _config.Is2014Client = lastUpdate > lastOldClientUpdate;

        if (!_config.Is2014Client)
            _directory = new DirectoryInfo(_directory).Parent?.FullName;

        _config.LastClientUpdate = lastUpdate.ToUnixTimestamp();

        _dirSet = true;

        RunGame();
    }

    public void AskIfRestart()
    {
        if (!_config.StartLauncherOnCommand)
            if (_logger.Ask("The launcher is not set to restart on a related command being run, " +
                            "would you like to enable this?", true))
                _config.StartLauncherOnCommand = true;

        if (_config.StartLauncherOnCommand)
            LaunchGame();
    }

    private void RunGame()
    {
        if (!_appStart || !_dirSet)
            return;

        if (_lConfig.OverwriteGameConfig)
            WriteConfig();

        if (!_world.Crashed)
            LaunchGame();
    }

    public void LaunchGame()
    {
        _game = Process.Start(Path.Join(_directory, "launcher", "launcher.exe"));
        _logger.LogDebug("Running game on process: {GamePath}", _game?.ProcessName);
    }

    private void WriteConfig()
    {
        var directory = Path.Join(_directory, "game");
        var config = Path.Join(directory, "LocalBuildConfig.xml");

        _logger.LogInformation("Looking For Header In {Directory} Ending In {Header}.", directory,
            _lConfig.HeaderFolderFilter);

        var parentUri = new Uri(directory);
        var headerFolders = Directory.GetDirectories(directory, string.Empty, SearchOption.AllDirectories)
            .Select(d => Path.GetDirectoryName(d)?.ToLower())
            .Where(d => new Uri(new DirectoryInfo(d!).Parent?.FullName!) == parentUri).ToArray();

        var headerFolder = headerFolders.FirstOrDefault(a => a?.EndsWith(_lConfig.HeaderFolderFilter) == true);
        headerFolder = Path.GetFileName(headerFolder?.Remove(headerFolder.Length - _lConfig.HeaderFolderFilter.Length));

        _logger.LogDebug("Found header: {Header}", headerFolder);

        _logger.LogInformation("Writing Build Config To {Place}", config);

        var newDoc = new XDocument();
        var root = new XElement("MQBuildConfg");

        foreach (var item in GetConfigValues(headerFolder))
        {
            var xmlItem = new XElement("item");
            xmlItem.Add(new XAttribute("name", item.Key));
            xmlItem.Add(new XAttribute("value", item.Value));
            root.Add(xmlItem);
        }

        newDoc.Add(root);
        newDoc.Save(config);

        _logger.LogDebug("Written build configuration");
    }

    private Dictionary<string, string> GetConfigValues(string header) => new()
    {
        { $"{header}.unity.url.membership", $"{_lConfig.BaseUrl}/Membership" },
        { $"{header}.unity.cache.domain", $"{_lConfig.BaseUrl}/Cache" },
        { $"{header}.unity.cache.license", $"{_lConfig.CacheLicense}" },
        { $"{header}.unity.cache.size", _lConfig.CacheSize.ToString() },
        { $"{header}.unity.cache.expiration", _lConfig.CacheExpiration.ToString() },
        { "game.cacheversion", _lConfig.CacheVersion.ToString() },
        { $"{header}.unity.url.crisp.host", $"{_lConfig.BaseUrl}/Chat/" },
        { "asset.log", _lConfig.LogAssets ? "true" : "false" },
        { "asset.disableversioning", _lConfig.DisableVersions ? "true" : "false" },
        { "asset.jboss", $"{_lConfig.BaseUrl}/Apps/" },
        { "asset.bundle", $"{_lConfig.BaseUrl}/Client/Bundles" },
        { "asset.audio", $"{_lConfig.BaseUrl}/Client/Audio" },
        { "logout.url", $"{_lConfig.BaseUrl}/Logout" },
        { "contactus.url", $"{_lConfig.BaseUrl}/Contact" },
        { "tools.urlbase", $"{_lConfig.BaseUrl}/Tools/" },
        { "leaderboard.domain", $"{_lConfig.BaseUrl}/Apps/" },
        { "analytics.baseurl", $"{_lConfig.BaseUrl}/Analytics/" },
        { "analytics.enabled", _lConfig.AnalyticsEnabled ? "true" : "false" },
        { "analytics.apikey", _lConfig.AnalyticsApiKey },
        { "project.name", _lConfig.ProjectName }
    };
}
