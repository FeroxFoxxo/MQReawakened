using Server.Base.Core.Abstractions;

namespace Server.Web.Models;

public class WebRConfig : IRConfig
{
    public List<string> IgnorePaths { get; }

    public WebRConfig() =>
        IgnorePaths = [];
}
