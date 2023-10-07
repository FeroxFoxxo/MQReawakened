using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Launcher.Controllers.EN.DLC;

[Route("/en/dlc/login")]
public class LoginController(ILogger<LoginController> logger) : Controller
{
    [HttpGet]
    public IActionResult GetLoginForm()
    {
        logger.LogError("User is using a 2013 launcher, which is not yet supported.");

        return Ok();
    }
}
