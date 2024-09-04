using Microsoft.AspNetCore.Mvc.RazorPages;
using Server.Base.Core.Configs;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Commands;

namespace Web.Razor.Pages;

public class CommandsModel(InternalInbuiltCommands cCommands, MQRSlashCommands sCommands, InternalRwConfig config) : PageModel
{
    public List<CommandModel> ClientCommands => cCommands.ClientCommands;
    public List<CommandModel> ServerCommands => sCommands.ServerCommands;

    public void OnGet() => ViewData["ServerName"] = config.ServerName;
}
