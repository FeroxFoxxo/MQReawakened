using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Reawakened.Players.Helpers;

namespace Web.Razor.Pages;

public class IndexModel(InternalRwConfig config, PlayerContainer container) : PageModel
{
    public string ServerName => config.ServerName;
    public int PlayerCount => container.GetPlayerCount();

    public void OnGet() => ViewData["ServerName"] = config.ServerName;
}
