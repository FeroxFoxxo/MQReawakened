using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Commands;

namespace Web.Razor.Pages;

public class CommandsModel(InternalInbuiltCommands cCommands, MQRSlashCommands sCommands) : PageModel
{
    public List<CommandModel> ClientCommands => cCommands.ClientCommands;
    public List<CommandModel> ServerCommands => sCommands.ServerCommands;

    public static void OnGet()
    {
    }
}
