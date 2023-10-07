using Server.Base.Core.Abstractions;
using Server.Web.Models;

namespace Web.AssetBundles.Services;

public class AddAssetBypass(WebRConfig config) : IService
{
    private readonly WebRConfig _config = config;

    public void Initialize() => _config.IgnorePaths.Add("/Client/");
}
