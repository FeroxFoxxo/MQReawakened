using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Network.Enums;
using Server.Base.Worlds;
using Server.Reawakened.Configs;
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

public class StartGame(EventSink sink, IHostApplicationLifetime appLifetime, ILogger<StartGame> logger, ServerConsole _console,
    World world, PlayerEventSink playerEventSink, RandomKeyGenerator generator,
    LauncherRConfig lConfig, LauncherRwConfig lWConfig, InternalRwConfig ilWConfig, ServerRConfig sConfig) : IService
{
    private string _directory;
    private bool _dirSet = false, _appStart = false;
    private Process _game;

    public PackageInformation CurrentVersion { get; private set; }

    public void Initialize()
    {
        appLifetime.ApplicationStarted.Register(AppStarted);
        sink.WorldLoad += GetGameInformation;
        sink.Shutdown += StopGame;
        sink.ChangedOperationalMode += CheckRemakeConfig;
        playerEventSink.PlayerRefreshed += AskIfRestart;
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

        logger.LogDebug("Getting the game executable...");

        try
        {
            lWConfig.GameSettingsFile = SetFileValue.SetIfNotNull(lWConfig.GameSettingsFile, "Get Settings File",
                "Settings File (*.txt)\0*.txt\0");
        }
        catch
        {
            // ignored
        }

        while (true)
        {
            if (string.IsNullOrEmpty(lWConfig.GameSettingsFile) || !lWConfig.GameSettingsFile.EndsWith("settings.txt"))
            {
                logger.LogError("Please enter the absolute file path for your game's 'settings.txt' file.");
                lWConfig.GameSettingsFile = Console.ReadLine();
                continue;
            }

            _directory = Path.GetDirectoryName(lWConfig.GameSettingsFile);

            if (string.IsNullOrEmpty(_directory))
                continue;

            CurrentVersion =
                JsonSerializer.Deserialize<PackageInformation>(File.ReadAllText(Path.Join(_directory, "current.txt")));

            break;
        }

        logger.LogInformation("Launcher Directory: {Directory}", Path.GetDirectoryName(lWConfig.GameSettingsFile));

        var lastUpdate = DateTime.ParseExact(CurrentVersion.game.lastUpdate, lConfig.TimeFilter,
            CultureInfo.InvariantCulture);
        var majorClientUpdate = DateTime.ParseExact(lConfig.MajorClientUpdate, lConfig.TimeFilter,
            CultureInfo.InvariantCulture);

        sConfig.Is2014Client = lastUpdate > majorClientUpdate;

        lWConfig.LastClientUpdate = lastUpdate.ToUnixTimestamp();
        lWConfig.MajorClientUpdate = majorClientUpdate.ToUnixTimestamp();

        if (string.IsNullOrEmpty(lWConfig.AnalyticsApiKey))
        {
            lWConfig.AnalyticsApiKey = generator.GetRandomKey<Analytics>(string.Empty);
            logger.LogDebug("Set API key to: {ApiKey}", lWConfig.AnalyticsApiKey);
        }

        _dirSet = true;

        RunGame();
    }

    public void AskIfRestart()
    {
        if (!ShouldRun())
            return;

        if (!lWConfig.StartLauncherOnCommand)
            if (logger.Ask("The launcher is not set to restart on a related command being run, " +
                            "would you like to enable this?", true))
                lWConfig.StartLauncherOnCommand = true;

        if (lWConfig.StartLauncherOnCommand)
            LaunchGame();
    }

    public bool ShouldRun()
    {
        if (ilWConfig.NetworkType.HasFlag(NetworkType.Client))
            return true;

        logger.LogWarning("NOT RUNNING GAME: SERVER IS HEADLESS");
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
            logger.LogCritical("Server ran into a critical error during execution. " +
                                "The game will not start until this is resolved.");
            return;
        }

        if (lConfig.OverwriteGameConfig)
        {
            SetSettings();
            WriteConfig();
        }

        if (!world.Crashed)
            LaunchGame();
    }

    public void SetSettings()
    {
        if (lWConfig.GameSettingsFile == null)
            return;

        dynamic settings = JsonConvert.DeserializeObject<ExpandoObject>(File.ReadAllText(lWConfig.GameSettingsFile))!;
        settings.launcher.baseUrl = ilWConfig.GetHostAddress();
        settings.launcher.fullscreen = lConfig.Fullscreen ? "true" : "false";
        settings.launcher.onGameClosePopup = lConfig.OnGameClosePopup ? "true" : "false";
        settings.patcher.baseUrl = ilWConfig.GetHostAddress();
        File.WriteAllText(lWConfig.GameSettingsFile, JsonConvert.SerializeObject(settings));
    }

    public void LaunchGame()
    {
        _game = Process.Start(Path.Join(_directory, "launcher", "launcher.exe"));
        logger.LogInformation("Running game on process: {GamePath}", _game?.ProcessName);
    }

    private void WriteConfig()
    {
        var directory = Path.Join(_directory, "game");
        var config = Path.Join(directory, "LocalBuildConfig.xml");

        logger.LogDebug("Looking For Header In {Directory} Ending In {Header}.", directory,
            lConfig.HeaderFolderFilter);

        var parentUri = new Uri(directory);
        var headerFolders = Directory.GetDirectories(directory, string.Empty, SearchOption.AllDirectories)
            .Select(d => Path.GetDirectoryName(d)?.ToLower())
            .Where(d => new Uri(new DirectoryInfo(d!).Parent?.FullName!) == parentUri).ToArray();

        var headerFolder = headerFolders.FirstOrDefault(a => a?.EndsWith(lConfig.HeaderFolderFilter) == true);
        headerFolder = Path.GetFileName(headerFolder?.Remove(headerFolder.Length - lConfig.HeaderFolderFilter.Length));

        logger.LogDebug("Found header: {Header}", headerFolder);

        logger.LogInformation("Writing Build Config To {Place}", config);

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

        logger.LogDebug("Written build configuration");
    }

    private Dictionary<string, string> GetConfigValues(string header) => new()
    {
        { $"{header}.unity.url.membership", $"{lConfig.ServerBaseUrl1}/Membership" },
        { $"{header}.unity.cache.domain", $"{lConfig.ServerBaseUrl1}/Cache" },
        { $"{header}.unity.cache.license", $"{lConfig.CacheLicense}" },
        { $"{header}.unity.cache.size", lConfig.CacheSize.ToString() },
        { $"{header}.unity.cache.expiration", lConfig.CacheExpiration.ToString() },
        { "game.cacheversion", lConfig.CacheVersion.ToString() },
        { $"{header}.unity.url.crisp.host", $"{lConfig.ServerBaseUrl1}/Chat/" },
        { "asset.log", lConfig.LogAssets ? "true" : "false" },
        { "asset.disableversioning", lConfig.DisableVersions ? "true" : "false" },
        { "asset.jboss", $"{lConfig.ServerBaseUrl1}/Apps{(sConfig.Is2014Client ? "/" : string.Empty)}" },
        { "asset.bundle", $"{lConfig.ServerBaseUrl1}/Client/Bundles" },
        { "asset.audio", $"{lConfig.ServerBaseUrl1}/Client/Audio" },
        { "logout.url", $"{lConfig.ServerBaseUrl1}/Logout" },
        { "contactus.url", $"{lConfig.ServerBaseUrl1}/Contact" },
        { "tools.urlbase", $"{lConfig.ServerBaseUrl1}/Tools/" },
        { "leaderboard.domain", $"{lConfig.ServerBaseUrl1}/Apps/" },
        { "analytics.baseurl", $"{lConfig.ServerBaseUrl1}/Analytics/" },
        { "analytics.enabled", lConfig.AnalyticsEnabled ? "true" : "false" },
        { "analytics.apikey", lWConfig.AnalyticsApiKey },
        { "project.name", lConfig.ProjectName }
    };
}
