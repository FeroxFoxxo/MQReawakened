using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;

namespace Web.Razor.Pages;

public class CollidersModel(InternalRwConfig config) : PageModel
{
    public void OnGet() => ViewData["ServerName"] = config.ServerName;
}
