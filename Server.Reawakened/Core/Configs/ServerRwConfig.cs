using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Core.Configs;

public class ServerRwConfig : IRwConfig
{
    public string CurrentEventOverride { get; set; }
    public string CurrentTimedEventOverride { get; set; }
    public string DomainName { get; set; }

    public ServerRwConfig()
    {
        CurrentEventOverride = string.Empty;
        CurrentTimedEventOverride = string.Empty;
        DomainName = string.Empty;
    }
}
