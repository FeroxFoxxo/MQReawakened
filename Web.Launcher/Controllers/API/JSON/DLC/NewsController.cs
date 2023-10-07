using Microsoft.AspNetCore.Mvc;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/news")]
public class NewsController(LauncherRConfig config) : Controller
{
    [HttpGet]
    public IActionResult GetNews() => Ok(config.News);
}
