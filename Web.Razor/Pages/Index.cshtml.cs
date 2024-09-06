using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;

namespace Web.Razor.Pages;

public class IndexModel(InternalRwConfig config) : PageModel
{
    public string ServerName => config.ServerName;

    public void OnGet() => ViewData["ServerName"] = config.ServerName;
}
