using Server.Base.Core.Abstractions;
using Server.Web.Models;

namespace Web.Apps.Analytics.Services;

public class AddAnalyticsBypass(WebRConfig config) : IService
{
    public void Initialize() => config.IgnorePaths.Add("/Analytics/");
}
