using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Commands;

namespace Web.Razor.Pages.Commands;

public class CommandListModel(InternalInbuiltCommands commands) : PageModel
{
    public List<CommandModel> Commands => commands.Commands;

    public static void OnGet()
    {
    }
}
