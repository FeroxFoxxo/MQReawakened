using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Base.Database;
using Server.Reawakened.Players.Helpers;

namespace Web.Razor.Pages;

public class IndexModel(InternalRwConfig config, PlayerContainer container, BaseDatabase database) : PageModel
{
    public string ServerName => config.ServerName;
    public int PlayerCount => container.GetPlayerCount();
    public int AccountCount => database.Accounts.Count();

    public void OnGet() => ViewData["ServerName"] = config.ServerName;
}
