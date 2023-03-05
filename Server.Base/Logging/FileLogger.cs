using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Logging.Internal;
using System.Text;

namespace Server.Base.Logging;

public class FileLogger
{
    private readonly InternalRConfig _config;
    private readonly Dictionary<string, ConsoleFileLogger> _fileLoggers;
    private readonly ILogger<FileLogger> _fLogger;
    private readonly object _lock;
    private readonly ILoggerFactory _loggerFactory;

    public FileLogger(ILoggerFactory loggerFactory, InternalRConfig config, ILogger<FileLogger> fLogger)
    {
        _loggerFactory = loggerFactory;
        _config = config;
        _fLogger = fLogger;
        _fileLoggers = new Dictionary<string, ConsoleFileLogger>();
        _lock = new object();
    }

    public void WriteGenericLog<T>(string logFileName, string title, string message, LoggerType type)
    {
        var builder = new StringBuilder()
            .AppendLine(title)
            .Append(message);

        WriteToFile<T>(logFileName, builder, type);
    }

    private void WriteToFile<T>(string fileName, StringBuilder builder, LoggerType type)
    {
        var message = builder.ToString();
        fileName = $"{fileName}.log";

        ILogger logger;

        try
        {
            logger = _loggerFactory.CreateLogger<T>();
        }
        catch (ObjectDisposedException)
        {
            logger = _fLogger;
        }

        try
        {
            lock (_lock)
            {
                if (!_fileLoggers.ContainsKey(fileName))
                    _fileLoggers.Add(fileName, new ConsoleFileLogger(fileName, _config));

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
