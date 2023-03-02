using Server.Base.Core.Abstractions;

namespace Server.Web.Models;

public class WebRConfig : IRwConfig
{
    public List<string> IgnorePaths { get; }

    public WebRConfig() =>
        IgnorePaths = new List<string>();
}
