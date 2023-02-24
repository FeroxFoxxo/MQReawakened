using Microsoft.AspNetCore.Mvc;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/news")]
public class NewsController : Controller
{
    private readonly LauncherRConfig _config;

    public NewsController(LauncherRConfig config) => _config = config;

    [HttpGet]
    public IActionResult GetNews() => Ok(_config.News);
}
