using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Logging.Internal;
using System.Text;

namespace Server.Base.Logging;

public class FileLogger
{
    private readonly Dictionary<string, ConsoleFileLogger> _fileLoggers;
    private readonly ILoggerFactory _loggerFactory;
    private readonly InternalRConfig _config;
    private readonly ILogger<FileLogger> _fLogger;
    private readonly object _lock;

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

        var logger = _loggerFactory.CreateLogger<T>();

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
                _fLogger.LogError("{Information}", message);
                break;
            case LoggerType.Warning:
                _fLogger.LogWarning("{Information}", message);
                break;
            case LoggerType.Debug:
                _fLogger.LogDebug("{Information}", message);
                break;
            case LoggerType.Trace:
                _fLogger.LogTrace("{Information}", message);
                break;
        }
    }
}
