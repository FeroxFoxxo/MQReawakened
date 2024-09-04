using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;

namespace Web.Razor.Pages.En.SignUp;

public class NewsModel(InternalRwConfig iConfig) : PageModel
{
    public void OnGet() => ViewData["ServerName"] = iConfig.ServerName;
}
