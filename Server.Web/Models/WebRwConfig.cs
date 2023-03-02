using Server.Base.Core.Abstractions;

namespace Server.Web.Models;

public class WebRwConfig : IRConfig
{
    public bool ShouldConcat { get; }

    public WebRwConfig() =>
        ShouldConcat = true;
}
