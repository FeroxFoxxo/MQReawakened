using Microsoft.AspNetCore.Mvc;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/news")]
public class NewsController : Controller
{
    private readonly LauncherStaticConfig _config;

    public NewsController(LauncherStaticConfig config) => _config = config;

    [HttpGet]
    public IActionResult GetNews() => Ok(_config.News);
}
