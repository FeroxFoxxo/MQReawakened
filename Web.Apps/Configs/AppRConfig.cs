using Server.Base.Core.Abstractions;

namespace Web.Apps.Configs;

public class AppRConfig : IRConfig
{
    public bool LogOmniture { get; set; }

    public AppRConfig() => LogOmniture = false;
}
