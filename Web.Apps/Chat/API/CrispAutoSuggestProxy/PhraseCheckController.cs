using Microsoft.AspNetCore.Mvc;

spAutoSuggestProxy;

[Route("/Chat/CrispAutoSuggestProxy/PhraseCheck")]
public class PhraseCheckController : Controller
{
    [HttpGet]
    public IActionResult GetPhraseCheck([FromQuery] string message) => Ok(message);
}
