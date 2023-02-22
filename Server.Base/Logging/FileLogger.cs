using Microsoft.Extensions.Logging;
using Server.Base.Logging.Internal;
using Server.Base.Network;
using System.Text;

namespace Server.Base.Logging;

public class FileLogger
{
    private readonly Dictionary<string, Internal.FileLogger> _fileLoggers;
    private readonly ILogger<FileLogger> _logger;
    private readonly object _lock;

    public FileLogger(ILogger<FileLogger> logger)
    {
        _logger = logger;
        _fileLoggers = new Dictionary<string, Internal.FileLogger>();
        _lock = new object();
    }
    
    public void WriteGenericLog<T>(string logFileName, string name, string message, LoggerType type)
    {
        var builder = new StringBuilder()
            .AppendLine($"# {DateTime.UtcNow} @ {name}:")
            .AppendLine()
            .AppendLine(message);

        WriteToFile<T>(logFileName, builder, type);
    }

    public void WriteNetStateLog<T>(string logFileName, NetState netState, string message, LoggerType type)
    {
        var builder = new StringBuilder()
            .AppendLine($"{DateTime.UtcNow}\t" +
                        $"{netState}\t" +
                        $"{message}"
            );

        WriteToFile<T>(logFileName, builder, type);
    }

    private void WriteToFile<T>(string fileName, StringBuilder builder, LoggerType type)
    {
        var message = builder.ToString();
        fileName = $"{fileName}.log";

        try
        {
            lock (_lock)
            {
                if (!_fileLoggers.ContainsKey(fileName))
                    _fileLoggers.Add(fileName, new Internal.FileLogger(fileName));

                _fileLoggers[fileName].WriteLine(message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not log file {NAME}", fileName);
        }

        switch (type)
        {
            case LoggerType.Error:
                _logger.LogError("{Name}: {Information}", typeof(T).Name, message);
                break;
            case LoggerType.Warning:
                _logger.LogWarning("{Name}: {Information}", typeof(T).Name, message);
                break;
            case LoggerType.Debug:
                _logger.LogDebug("{Name}: {Information}", typeof(T).Name, message);
                break;
            case LoggerType.Trace:
                _logger.LogTrace("{Name}: {Information}", typeof(T).Name, message);
                break;
        }
    }
}
