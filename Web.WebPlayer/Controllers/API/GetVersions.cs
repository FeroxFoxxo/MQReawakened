using Microsoft.AspNetCore.Mvc;
using Web.WebPlayer.Configs;
using Web.WebPlayer.Models;
using Web.WebPlayer.Services;

namespace Web.WebPlayer.Controllers.API;

[Route("api/getVersions")]
public class GetVersions(LoadGameClients loadGameClients, WebPlayerRwConfig rwConfig) : Controller
{
    [HttpGet]
    public IActionResult GetGameVersions()
    {
        var firstVersion = rwConfig.DefaultWebPlayer;
        var versions = loadGameClients.ClientFiles.Keys.ToArray();

        var versionList = new VersionListModel()
        {
            DefaultVersion = firstVersion,
            Versions = versions
        };

        return Ok(versionList);
    }
}
