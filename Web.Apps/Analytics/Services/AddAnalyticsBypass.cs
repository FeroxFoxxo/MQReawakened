using Server.Base.Core.Abstractions;
using Server.Web.Models;

namespace Web.Apps.Analytics.Services;

public class AddAnalyticsBypass : IService
{
    private readonly WebRConfig _config;

    public AddAnalyticsBypass(WebRConfig config) => _config = config;

    public void Initialize() => _config.IgnorePaths.Add("/Analytics/");
}
