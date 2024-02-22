using Microsoft.AspNetCore.Mvc;
using FileIO = System.IO.File;

namespace Web.WebPlayer.Controllers.API;

[Route("api/getGame/{gameVersion}")]
public class GetGame() : Controller
{
    [HttpGet]
    public IActionResult GetGameExecutable([FromRoute] string gameVersion)
    {
        return Ok();
        //return new FileContentResult(FileIO.ReadAllBytes(path), "application/octet-stream");
    }
}
