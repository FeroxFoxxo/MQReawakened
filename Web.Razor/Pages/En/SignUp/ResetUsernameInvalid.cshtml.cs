using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;

namespace Web.Razor.Pages.En.SignUp;

public class ResetUsernameInvalidModel(InternalRwConfig config) : PageModel
{
    public void OnGet() => ViewData["ServerName"] = config.ServerName;
}
