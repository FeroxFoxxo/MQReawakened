using Microsoft.AspNetCore.Mvc;

namespace Web.Launcher.Controllers.EN.DLC;

[Route("/en/dlc/login")]
public class LoginController : Controller
{
    [HttpGet]
    public IActionResult GetLoginForm() => Ok();
}
