using Server.Base.Core.Abstractions;

namespace Server.Web.Models;

public class WebRConfig : IRConfig
{
    public bool ShouldConcat { get; set; }

    public WebRConfig() =>
        ShouldConcat = true;
}
