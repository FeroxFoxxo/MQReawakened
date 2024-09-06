using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;

namespace Web.Razor.Pages.Account;

public class AccessDeniedModel(InternalRwConfig config) : PageModel
{
    public void OnGet() => ViewData["ServerName"] = config.ServerName;
}
