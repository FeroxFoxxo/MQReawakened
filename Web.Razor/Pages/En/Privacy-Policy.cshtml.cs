using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;

namespace Web.Razor.Pages.En;

public class Privacy_PolicyModel(InternalRwConfig iConfig) : PageModel
{
    public void OnGet() => ViewData["ServerName"] = iConfig.ServerName;
}
