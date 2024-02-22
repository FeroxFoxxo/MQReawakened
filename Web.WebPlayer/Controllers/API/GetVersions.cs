using Microsoft.AspNetCore.Mvc;

namespace Web.WebPlayer.Controllers.API;

[Route("api/getVersions")]
public class GetVersions() : Controller
{
    [HttpGet]
    public IActionResult GetGameVersions() => Ok();
}
