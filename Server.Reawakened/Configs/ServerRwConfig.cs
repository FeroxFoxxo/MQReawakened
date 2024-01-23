using Server.Base.Core.Abstractions;
using Server.Base.Network.Enums;

namespace Server.Reawakened.Configs;

public class ServerRwConfig : IRwConfig
{
    public string Current2014Event { get; set; }
    public string Current2013Event { get; set; }

    public ServerRwConfig()
    {
        Current2014Event = "EVT" + "_2014_" + "Sponge" + "Bob";
        Current2013Event = "EVT" + "_2013_" + "TM" + "NT" + "01";
    }
}
