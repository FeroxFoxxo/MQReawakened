using Server.Base.Core.Abstractions;

namespace Server.Web.Models;

public class WebConfig : IStaticConfig
{
    public bool ShouldConcat { get; set; }

    public WebConfig() =>
        ShouldConcat = true;
}
