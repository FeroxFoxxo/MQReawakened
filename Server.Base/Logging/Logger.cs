using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using System.Collections;

namespace Server.Base.Logging;

public class Logger(string categoryName) : ILogger
{
    private const LogLevel Level = LogLevel.Trace;
    public const string DateFormat = "[hh:mm:ss] ";

    private static readonly Stack<ConsoleColor> ConsoleColors = new();

    private static bool _criticalErrored;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception ex,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, ex);

        if (ex != null)
        {
            WriteLine(ConsoleColor.Red, message, "E", eventId.Id);
            LogException(ex);
        }
        else
        {
            var color = logLevel switch
            {
                LogLevel.Trace => ConsoleColor.Magenta,
                LogLevel.Debug => ConsoleColor.DarkCyan,
                LogLevel.Information => ConsoleColor.Cyan,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.DarkMagenta
            };

            var shortLogLevel = logLevel switch
            {
                LogLevel.Trace => "T",
                LogLevel.Debug => "D",
                LogLevel.Information => "I",
                LogLevel.Warning => "W",
                LogLevel.Error => "E",
                LogLevel.Critical => "C",
                _ => "U"
            };

            if (logLevel == LogLevel.Critical)
                _criticalErrored = true;

            WriteLine(color, message, shortLogLevel, eventId.Id);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= Level;

    public IDisposable BeginScope<TState>(TState state) => new NoopDisposable();

    private void WriteLine(ConsoleColor color, string message, string shortLogLevel, int eventId)
    {
        var prefix = $"{DateTime.UtcNow.ToString(DateFormat)}[{shortLogLevel}] {categoryName.Split('.').Last()}[{eventId}]";

        lock (((ICollection)ConsoleColors).SyncRoot)
        {
            PushColor(color);

            foreach (var msg in message.Split('\n'))
                Console.WriteLine($"{prefix}: {msg}");

            PopColor();
        }
    }

    private void PushColor(ConsoleColor color)
    {
        try
        {
            lock (((ICollection)ConsoleColors).SyncRoot)
            {
                ConsoleColors.Push(Console.ForegroundColor);

                Console.ForegroundColor = color;
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private void PopColor()
    {
        try
        {
            lock (((ICollection)ConsoleColors).SyncRoot)
                Console.ForegroundColor = ConsoleColors.Pop();
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private void LogException(Exception ex)
    {
        WriteLine(ConsoleColor.DarkRed, $"{ex}", "C", ex.HResult);

        var fileName = $"{DateTime.UtcNow.ToShortDateString().Replace('/', '_')}.log";
        var exceptionDir = InternalDirectory.GetDirectory("Logs/Exceptions");
        var file = Path.Combine(exceptionDir, fileName);
        using var fStream = File.Open(file, FileMode.OpenOrCreate);
        using var stream = new StreamWriter(fStream);

        stream.WriteLine($"Exception Caught: {DateTime.UtcNow}");
        stream.WriteLine(ex);
        stream.WriteLine();
    }

    public static bool HasCriticallyErrored() => _criticalErrored;
}
