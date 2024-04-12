using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using static LogFacade;

namespace Server.Reawakened.Core.Services;

public class ReawakenedLogger(ILogger<ReawakenedLogger> logger) : ILogger, IService
{
    public void Initialize()
    {
        setLogLevel(Level.DEBUG);
        setLogger(this);
    }

    public override void error(string str, object objectContext) =>
        logger.LogError("{Str}: {Context}", str, objectContext != null ? objectContext.ToString() : "N/A");

    public override void warn(string str, object objectContext) =>
        logger.LogWarning("{Str}: {Context}", str, objectContext != null ? objectContext.ToString() : "N/A");

    public override void info(string str, object objectContext) =>
        logger.LogInformation("{Str}: {Context}", str, objectContext != null ? objectContext.ToString() : "N/A");

    public override void debug(string str, object objectContext) =>
        logger.LogTrace("{Str}: {Context}", str, objectContext != null ? objectContext.ToString() : "N/A");
}
