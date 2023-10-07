namespace Web.Apps.Analytics.Models;

public class CommonProperties(ulong sessionId, DateTimeOffset timestamp)
{
    public ulong SessionId { get; set; } = sessionId;
    public DateTimeOffset Timestamp { get; set; } = timestamp;
}
