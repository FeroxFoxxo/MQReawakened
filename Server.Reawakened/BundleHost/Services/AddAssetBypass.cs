using Server.Base.Core.Abstractions;
using Server.Web.Models;

namespace Server.Reawakened.BundleHost.Services;

public class AddAssetBypass(WebRConfig _config) : IService
{
    public void Initialize() => _config.IgnorePaths.Add("/Client/");
}
