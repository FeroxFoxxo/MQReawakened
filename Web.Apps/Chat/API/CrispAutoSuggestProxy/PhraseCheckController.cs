using Microsoft.AspNetCore.Mvc;
using Web.Apps.Chat.Services;

namespace Web.Apps.Chat.API.CrispAutoSuggestProxy;

[Route("/Chat/CrispAutoSuggestProxy/PhraseCheck")]
public class PhraseCheckController : Controller
{
    [HttpGet]
    public IActionResult GetPhraseCheck([FromQuery] string message) => Ok(message);
}
