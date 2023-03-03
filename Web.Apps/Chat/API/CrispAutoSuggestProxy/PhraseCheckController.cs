using Microsoft.AspNetCore.Mvc;

namespace Web.Apps.Chat.API.CrispAutoSuggestProxy;

[Route("/Chat/CrispAutoSuggestProxy/PhraseCheck")]
public class PhraseCheckController : Controller
{
    [HttpGet]
    public IActionResult GetPhraseCheck([FromQuery] string message) => Ok(message);
}
