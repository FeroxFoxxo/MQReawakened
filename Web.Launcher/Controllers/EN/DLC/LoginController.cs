using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Launcher.Controllers.EN.DLC;

[Route("/en/dlc/login")]
public class LoginController : Controller
{
    private readonly ILogger<LoginController> _logger;

    public LoginController(ILogger<LoginController> logger) => _logger = logger;

    [HttpGet]
    public IActionResult GetLoginForm()
    {
        _logger.LogError("User is using a 2013 launcher, which is not yet supported.");

        return Ok();
    }
}
