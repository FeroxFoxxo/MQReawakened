using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.BundleHost.Configs;
using ShellProgressBar;

namespace Server.Reawakened.BundleHost.Models;

public class DefaultProgressBar : IDisposable
{
    private readonly ChildProgressBar _bottomBar;

    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly bool _logProgressAfter;
    private readonly List<string> _messages;
    private readonly ProgressBar _topBar;

    public DefaultProgressBar(int count, string message, Microsoft.Extensions.Logging.ILogger logger,
        AssetBundleRwConfig config)
    {
        _logger = logger;
        _logProgressAfter = config.LogProgressBars;
        _messages = [];

        var bottomBarOptions = new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.DarkYellow,
            ForegroundColorDone = ConsoleColor.DarkGreen,
            ProgressCharacter = '─',
            ProgressBarOnBottom = true
        };

        var topBarOptions = bottomBarOptions.DeepCopy();
        topBarOptions.DisableBottomPercentage = true;

        _topBar = new ProgressBar(count, string.Empty, topBarOptions);
        _bottomBar = _topBar.Spawn(count, message, bottomBarOptions);
    }

    public void Dispose()
    {
        _bottomBar.AsProgress<float>().Report(1f);
        _topBar.AsProgress<float>().Report(1f);

        _bottomBar?.Dispose();
        _topBar?.Dispose();

        if (_logProgressAfter)
        {
            _logger.LogTrace("\n-- Log of progress bar --");

            foreach (var message in _messages)
                _logger.LogTrace("{Message}", message);

            _logger.LogTrace("-------------------------\n");
        }

        GC.SuppressFinalize(this);
    }

    public void TickBar()
    {
        _topBar.Tick();
        _bottomBar.Tick();
    }

    public void SetMessage(string message)
    {
        _bottomBar.Message = message;
        _messages.Add(message);
    }
}
