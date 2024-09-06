using Server.Base.Core.Abstractions;

namespace Web.WebPlayer.Configs;

public class WebPlayerRwConfig : IRwConfig
{
    public string DefaultWebPlayer { get; set; }

    public WebPlayerRwConfig() =>
        DefaultWebPlayer = string.Empty;
}
