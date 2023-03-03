using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Network.Enums;
using Server.Base.Worlds;
using Server.Reawakened.Network.Services;
using Server.Reawakened.Players.Events;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Xml.Linq;
using Web.Launcher.Models;
using Web.Launcher.Models.Current;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Web.Launcher.Services;

public class StartGame : IService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ServerConsole _console;
    private readonly RandomKeyGenerator _generator;
    private readonly InternalRwConfig _internalWConfig;
    private readonly ILogger<StartGame> _logger;
    private readonly PlayerEventSink _playerEventSink;
    private readonly LauncherRConfig _lConfig;
    private readonly LauncherRwConfig _lWConfig;
    private readonly EventSink _sink;
    private readonly World _world;

    private string _directory;
    private bool _dirSet, _appStart;
    private Process _game;

    public PackageInformation CurrentVersion { get; private set; }

    public StartGame(EventSink sink, LauncherRConfig lConfig, IHostApplicationLifetime appLifetime,
        ILogger<StartGame> logger, ServerConsole console, World world, LauncherRwConfig lWConfig,
        PlayerEventSink playerEventSink, RandomKeyGenerator generator, InternalRwConfig internalWConfig)
    {
        _sink = sink;
        _lConfig = lConfig;
        _appLifetime = appLifetime;
        _logger = logger;
        _console = console;
        _world = world;
        _lWConfig = lWConfig;
        _playerEventSink = playerEventSink;
        _generator = generator;
        _internalWConfig = internalWConfig;

        _dirSet = false;
        _appStart = false;
    }

    public void Initialize()
    {
        _appLifetime.ApplicationStarted.Register(AppStarted);
        _sink.WorldLoad += GetGameInformation;
        _sink.Shutdown += StopGame;
        _sink.ChangedOperationalMode += CheckRemakeConfig;
        _playerEventSink.PlayerRefreshed += AskIfRestart;
    }

    private void CheckRemakeConfig()
    {
        if (!ShouldRun())
            return;

        RunGame();
    }

    private void StopGame() => _game?.CloseMainWindow();

    private void AppStarted()
    {
        _appStart = true;
        RunGame();
    }

    private void GetGameInformation()
    {
        _console.AddCommand(
            "runLauncher",
            "Runs the launcher and hooks it into the current process.",
            NetworkType.Client,
            _ => LaunchGame()
        );

        _logger.LogDebug("Getting the game executable...");

        try
        {
            _lWConfig.GameSettingsFile = SetFileValue.SetIfNotNull(_lWConfig.GameSettingsFile, "Get Settings File",
                "Settings File (*.txt)\0*.txt\0");
        }
        catch
        {
            // ignored
        }

        while (true)
        {
            if (string.IsNullOrEmpty(_lWConfig.GameSettingsFile) || !_lWConfig.GameSettingsFile.EndsWith("settings.txt"))
            {
                _logger.LogError("Please enter the absolute file path for your game's 'settings.txt' file.");
                _lWConfig.GameSettingsFile = Console.ReadLine();
                continue;
            }

            _directory = Path.GetDirectoryName(_lWConfig.GameSettingsFile);

            if (string.IsNullOrEmpty(_directory))
                continue;

            CurrentVersion =
                JsonSerializer.Deserialize<PackageInformation>(File.ReadAllText(Path.Join(_directory, "current.txt")));

            break;
        }

        _logger.LogInformation("Launcher Directory: {Directory}", Path.GetDirectoryName(_lWConfig.GameSettingsFile));

        var lastUpdate = DateTime.ParseExact(CurrentVersion.game.lastUpdate, _lConfig.TimeFilter,
            CultureInfo.InvariantCulture);
        var lastOldClientUpdate = DateTime.ParseExact(_lConfig.OldClientLastUpdate, _lConfig.TimeFilter,
            CultureInfo.InvariantCulture);

        _lWConfig.Is2014Client = lastUpdate > lastOldClientUpdate;

        if (!_lWConfig.Is2014Client)
            _directory = new DirectoryInfo(_directory).Parent?.FullName;

        _lWConfig.LastClientUpdate = lastUpdate.ToUnixTimestamp();

        if (string.IsNullOrEmpty(_lWConfig.AnalyticsApiKey))
        {
            _lWConfig.AnalyticsApiKey = _generator.GetRandomKey<Analytics>(string.Empty);
            _logger.LogDebug("Set API key to: {ApiKey}", _lWConfig.AnalyticsApiKey);
        }

        _dirSet = true;

        RunGame();
    }

    public void AskIfRestart()
    {
        if (!ShouldRun())
            return;

        if (!_lWConfig.StartLauncherOnCommand)
            if (_logger.Ask("The launcher is not set to restart on a related command being run, " +
                            "would you like to enable this?", true))
                _lWConfig.StartLauncherOnCommand = true;

        if (_lWConfig.StartLauncherOnCommand)
            LaunchGame();
    }

    public bool ShouldRun()
    {
        if (_internalWConfig.NetworkType.HasFlag(NetworkType.Client))
            return true;

        _logger.LogWarning("NOT RUNNING GAME: SERVER IS HEADLESS");
        return false;
    }

    private void RunGame()
    {
        if (!_appStart || !_dirSet)
            return;

        if (!ShouldRun())
            return;

        if (Logger.HasCriticallyErrored())
        {
            _logger.LogCritical("Server ran into a critical error during execution. " +
                                "The game will not start until this is resolved.");
            return;
        }

        if (_lConfig.OverwriteGameConfig)
        {
            SetSettings();
            WriteConfig();
        }

        if (!_world.Crashed)
            LaunchGame();
    }

    public void SetSettings()
    {
        if (_lWConfig.GameSettingsFile == null)
            return;

        dynamic settings = JsonConvert.DeserializeObject<ExpandoObject>(File.ReadAllText(_lWConfig.GameSettingsFile))!;
        settings.launcher.baseUrl = _internalWConfig.GetHostAddress();
        settings.launcher.fullscreen = _lConfig.Fullscreen ? "true" : "false";
        settings.launcher.onGameClosePopup = _lConfig.OnGameClosePopup ? "true" : "false";
        settings.patcher.baseUrl = _internalWConfig.GetHostAddress();
        File.WriteAllText(_lWConfig.GameSettingsFile, JsonConvert.SerializeObject(settings));
    }

    public void LaunchGame()
    {
        _game = Process.Start(Path.Join(_directory, "launcher", "launcher.exe"));
        _logger.LogInformation("Running game on process: {GamePath}", _game?.ProcessName);
    }

    private void WriteConfig()
    {
        var directory = Path.Join(_directory, "game");
        var config = Path.Join(directory, "LocalBuildConfig.xml");

        _logger.LogDebug("Looking For Header In {Directory} Ending In {Header}.", directory,
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
            if (string.IsNullOrEmpty(item.Key) || string.IsNullOrEmpty(item.Value))
                continue;

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
        { $"{header}.unity.url.membership", $"{_lConfig.ServerBaseUrl1}/Membership" },
        { $"{header}.unity.cache.domain", $"{_lConfig.ServerBaseUrl1}/Cache" },
        { $"{header}.unity.cache.license", $"{_lConfig.CacheLicense}" },
        { $"{header}.unity.cache.size", _lConfig.CacheSize.ToString() },
        { $"{header}.unity.cache.expiration", _lConfig.CacheExpiration.ToString() },
        { "game.cacheversion", _lConfig.CacheVersion.ToString() },
        { $"{header}.unity.url.crisp.host", $"{_lConfig.ServerBaseUrl1}/Chat/" },
        { "asset.log", _lConfig.LogAssets ? "true" : "false" },
        { "asset.disableversioning", _lConfig.DisableVersions ? "true" : "false" },
        { "asset.jboss", $"{_lConfig.ServerBaseUrl1}/Apps/" },
        { "asset.bundle", $"{_lConfig.ServerBaseUrl1}/Client/Bundles" },
        { "asset.audio", $"{_lConfig.ServerBaseUrl1}/Client/Audio" },
        { "logout.url", $"{_lConfig.ServerBaseUrl1}/Logout" },
        { "contactus.url", $"{_lConfig.ServerBaseUrl1}/Contact" },
        { "tools.urlbase", $"{_lConfig.ServerBaseUrl1}/Tools/" },
        { "leaderboard.domain", $"{_lConfig.ServerBaseUrl1}/Apps/" },
        { "analytics.baseurl", $"{_lConfig.ServerBaseUrl1}/Analytics/" },
        { "analytics.enabled", _lConfig.AnalyticsEnabled ? "true" : "false" },
        { "analytics.apikey", _lWConfig.AnalyticsApiKey },
        { "project.name", _lConfig.ProjectName }
    };
}
