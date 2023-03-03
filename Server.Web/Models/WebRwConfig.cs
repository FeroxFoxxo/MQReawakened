using Server.Base.Core.Abstractions;

namespace Server.Web.Models;

public class WebRwConfig : IRwConfig
{
    public bool ShouldConcat { get; }

    public WebRwConfig() =>
        ShouldConcat = true;
}
