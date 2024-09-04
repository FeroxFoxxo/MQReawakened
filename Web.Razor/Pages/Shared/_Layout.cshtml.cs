using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;

namespace Web.Razor.Pages.Shared;

public class _LayoutModel(InternalRwConfig config) : PageModel
{
    public string ServerName => config.ServerName;
}
