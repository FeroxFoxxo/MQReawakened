using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Reawakened.Core.Configs;

namespace Web.Razor.Templates;

public class UsernameResetTemplateModel(ServerRwConfig config, InternalRwConfig iConfig) : PageModel
{
    public string ResetLink { get; set; }

    public string Domain => config.DomainName;
    public string SiteName => iConfig.ServerName;
}
