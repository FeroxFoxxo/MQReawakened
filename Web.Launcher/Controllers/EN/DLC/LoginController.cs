using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Launcher.Controllers.EN.DLC;

[Route("/en/dlc/login")]
public class LoginController(ILogger<LoginController> logger) : Controller
{
    private readonly ILogger<LoginController> _logger = logger;

    [HttpGet]
    public IActionResult GetLoginForm()
    {
        _logger.LogError("User is using a 2013 launcher, which is not yet supported.");

        return Ok();
    }
}
