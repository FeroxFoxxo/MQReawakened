using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;

namespace Web.Razor.Pages.En;

public class ReportABugModel(InternalRwConfig iConfig) : PageModel
{
    public string ServerId => iConfig.DiscordServerId;
    public void OnGet() => ViewData["ServerName"] = iConfig.ServerName;
}
