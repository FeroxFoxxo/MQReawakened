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
    private readonly bool _useTextFallback;
    private readonly int _totalCount;
    private int _current;
    private int _fallbackEvery;
    private string _currentMessage;

    public DefaultProgressBar(int count, string message, Microsoft.Extensions.Logging.ILogger logger,
        AssetBundleRwConfig config)
    {
        _logger = logger;
        _logProgressAfter = config.LogProgressBars;
        _messages = [];
        _totalCount = count;
        _current = 0;
        _currentMessage = message;

        _useTextFallback = EnvironmentExt.IsContainerOrNonInteractive();

        if (_useTextFallback)
        {
            _fallbackEvery = Math.Max(1, count / 10);
            _logger.LogInformation("{Message} (0/{Total})", message, count);
        }
        else
        {
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
    }

    public void Dispose()
    {
        if (_useTextFallback)
        {
            _logger.LogInformation("{Message} ({Current}/{Total})", _currentMessage, _totalCount, _totalCount);
        }
        else
        {
            _bottomBar.AsProgress<float>().Report(1f);
            _topBar.AsProgress<float>().Report(1f);

            _bottomBar?.Dispose();
            _topBar?.Dispose();
        }

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
        if (_useTextFallback)
        {
            _current++;
            if (_current == 1 || _current % _fallbackEvery == 0 || _current >= _totalCount)
            {
                var percent = (int)Math.Round((double)_current * 100 / Math.Max(1, _totalCount));
                _logger.LogInformation("{Message} ({Current}/{Total}, {Percent}%)", _currentMessage, _current, _totalCount, percent);
            }
        }
        else
        {
            _topBar.Tick();
            _bottomBar.Tick();
        }
    }

    public void SetMessage(string message)
    {
        if (_useTextFallback)
        {
            _currentMessage = message;
            _messages.Add(message);
            _logger.LogInformation("{Message}", message);
            return;
        }

        _bottomBar.Message = message;
        _messages.Add(message);
    }
}
