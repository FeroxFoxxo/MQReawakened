using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Xml;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/news")]
public class NewsController(LauncherRConfig rConfig) : Controller
{
    [HttpGet]
    public IActionResult GetNews() => Ok(rConfig.News);
}
