using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Configs;

public class ServerRwConfig : IRwConfig
{
    public string CurrentEvent { get; set; }

    public ServerRwConfig() => CurrentEvent = "EVT" + "_2014_" + "Sponge" + "Bob";
}
