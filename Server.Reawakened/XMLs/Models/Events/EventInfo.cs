using Server.Reawakened.Players.Helpers;
using static A2m.Server.DescriptionHandler;

namespace Server.Reawakened.XMLs.Models.Events;

public class EventInfo
{
    public int EventId { get; set; }
    public bool DisplayAd { get; set; }
    public bool DisplayPopupMenu { get; set; }
    public bool DisplayPopupIcon { get; set; }
    public List<SecondaryEventInfo> SecondaryEvents { get; set; }
    public bool DisplayAutoPopup { get; set; }
    public string TimedEventName { get; set; }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('%');

        sb.Append(EventId);
        sb.Append(DisplayAd ? 1 : 0);
        sb.Append(DisplayPopupMenu ? 1 : 0);
        sb.Append(DisplayPopupIcon ? 1 : 0);

        var secEventSb = new SeparatedStringBuilder('|');

        foreach (var secondaryEvent in SecondaryEvents)
        {
            secEventSb.Append(secondaryEvent.EventId);
            secEventSb.Append(secondaryEvent.DisplayAd ? 1 : 0);
            secEventSb.Append(secondaryEvent.DisplayIcon ? 1 : 0);
        }

        sb.Append(secEventSb.ToString());

        sb.Append(DisplayAutoPopup ? 1 : 0);
        sb.Append(TimedEventName);

        return sb.ToString();
    }
}
