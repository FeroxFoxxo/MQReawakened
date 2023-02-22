namespace Web.Apps.Analytics.Models;

public class CommonProperties
{
    public ulong SessionId { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public CommonProperties(ulong sessionId, DateTimeOffset timestamp)
    {
        SessionId = sessionId;
        Timestamp = timestamp;
    }
}
