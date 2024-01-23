using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Configs;

public class ServerRwConfig : IRwConfig
{
    public string Current2014Event { get; set; }
    public string Current2013Event { get; set; }
    
    public string Current2014TimedEvent { get; set; }
    public string Current2013TimedEvent { get; set; }

    public ServerRwConfig()
    {
        var eventId2014 = "Sponge";
        var eventId2013 = "TM";

        eventId2014 += "Bob";
        eventId2013 += "NT";

        Current2013Event = "EVT" + "_2013_" + eventId2013 + "01";
        Current2014Event = "EVT" + "_2014_" + eventId2014;

        Current2014TimedEvent = eventId2014 + "Party" + "Event";
        Current2013TimedEvent = string.Empty;
    }
}
