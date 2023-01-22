using Microsoft.Extensions.Logging;
using Server.Base.Core.Helpers.Internal;
using ShellProgressBar;

namespace Web.AssetBundles.Models;

public class DefaultProgressBar : IDisposable
{
    private readonly ProgressBar _topBar;
    private readonly ChildProgressBar _bottomBar;

    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly List<string> _messages;

    public DefaultProgressBar(int count, string message, Microsoft.Extensions.Logging.ILogger logger)
    {
        _logger = logger;
        _messages = new List<string>();

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

        _topBar = new ProgressBar(count, "", topBarOptions);
        _bottomBar = _topBar.Spawn(count, message, bottomBarOptions);
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

    public void Dispose()
    {
        _bottomBar.AsProgress<float>().Report(1f);
        _topBar.AsProgress<float>().Report(1f);

        _bottomBar?.Dispose();
        _topBar?.Dispose();

        Console.WriteLine();

        _logger.LogTrace("-- Log of progress bar --");

        foreach (var message in _messages)
            _logger.LogTrace(message);

        _logger.LogTrace("-------------------------");

        Console.WriteLine();

        GC.SuppressFinalize(this);
    }
}
