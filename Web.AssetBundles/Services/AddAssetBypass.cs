using Server.Base.Core.Abstractions;
using Server.Web.Models;

namespace Web.AssetBundles.Services;

public class AddAssetBypass : IService
{
    private readonly WebRConfig _config;

    public AddAssetBypass(WebRConfig config) => _config = config;

    public void Initialize() => _config.IgnorePaths.Add("/Client/");
}
