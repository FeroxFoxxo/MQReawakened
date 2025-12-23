using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Reawakened.XMLs.Bundles.Internal;
using Web.Launcher.Models;

namespace Web.Razor.Pages.En;

public class NewsModel(InternalRwConfig iConfig, LauncherRConfig launcherRConfig) : PageModel
{
    public string News => launcherRConfig.News;

    public void OnGet() => ViewData["ServerName"] = iConfig.ServerName;
}
