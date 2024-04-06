namespace Server.Reawakened.XMLs.Data.Events;

public class Event
{
    public int EventId { get; set; }
    public string EventName { get; set; }
    public bool HasEventAd { get; set; }
    public bool HasEventPopupIcon { get; set; }
    public bool HasEventPopupMenu { get; set; }
}
