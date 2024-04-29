using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Commands;

namespace Web.Razor.Pages;

public class Commands(InternalInbuiltCommands commands) : PageModel
{
    public List<CommandModel> ClientCommands => commands.Commands;

    public static void OnGet()
    {
    }
}
