using Server.Base.Core.Abstractions;
using Server.Web.Models;

namespace Web.Apps.Chat.Services;

public class AddChatBypass : IService
{
    private readonly WebRConfig _config;

    public AddChatBypass(WebRConfig config) => _config = config;

    public void Initialize() => _config.IgnorePaths.Add("/Chat/");
}
