using Microsoft.AspNetCore.Mvc;
using Server.Base.Core.Configs;

namespace Web.WebPlayer.Controllers.API;

[Route("api/getHost")]
public class GetHost(InternalRwConfig config) : Controller
{
    [HttpGet]
    public IActionResult GetHostAddress() => Ok(config.GetHostName());
}
