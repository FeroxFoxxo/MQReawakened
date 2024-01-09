using Microsoft.Extensions.Logging;
using Server.Base.Core.Configs;
using Server.Base.Logging.Internal;
using System.Text;

namespace Server.Base.Logging;

public class FileLogger(ILoggerFactory loggerFactory, InternalRConfig config, ILogger<FileLogger> fLogger)
{
    private readonly Dictionary<string, ConsoleFileLogger> _fileLoggers = [];
    private readonly object _lock = new();

    public void WriteGenericLog<T>(string logFileName, string title, string message, LoggerType type)
    {
        var builder = new StringBuilder()
            .AppendLine(title)
            .Append(message);

        WriteToFile<T>(logFileName, builder, type);
    }

    private void WriteToFile<T>(string fileName, StringBuilder builder, LoggerType type)
    {
        var message = builder.ToString().TrimEnd('\r', '\n');
        fileName = $"{fileName}.log";

        ILogger logger;

        try
        {
            logger = loggerFactory.CreateLogger<T>();
        }
        catch (ObjectDisposedException)
        {
            logger = fLogger;
        }

        try
        {
            lock (_lock)
            {
                if (!_fileLoggers.ContainsKey(fileName))
                    _fileLoggers.Add(fileName, new ConsoleFileLogger(fileName, config));

                _fileLoggers[fileName].WriteLine($"# {DateTime.UtcNow} @ " + message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not log file {NAME}", fileName);
        }

        switch (type)
        {
            case LoggerType.Error:
                logger.LogError("{Information}", message);
                break;
            case LoggerType.Warning:
                logger.LogWarning("{Information}", message);
                break;
            case LoggerType.Debug:
                logger.LogDebug("{Information}", message);
                break;
            case LoggerType.Trace:
                logger.LogTrace("{Information}", message);
                break;
        }
    }
}
