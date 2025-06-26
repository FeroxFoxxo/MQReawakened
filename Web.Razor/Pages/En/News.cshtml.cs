using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;

namespace Web.Razor.Pages.En;

public class NewsModel(InternalRwConfig iConfig) : PageModel
{
    public static string News => $"You expected there to be news here? It's {DateTime.Now.Year}!";

    public void OnGet() => ViewData["ServerName"] = iConfig.ServerName;
}
