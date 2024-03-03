using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.WebPlayer.Configs;

public class WebPlayerRConfig : IRConfig
{
    public string GameFolder { get; }

    public WebPlayerRConfig() => GameFolder = InternalDirectory.GetDirectory("WebPlayers");
}
