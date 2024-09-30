using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Web.Launcher.Models;

namespace Web.Razor.Pages.En;

public class NewsModel(InternalRwConfig iConfig, LauncherRConfig conifg) : PageModel
{
    public string News => conifg.News;

    public void OnGet() => ViewData["ServerName"] = iConfig.ServerName;
}
