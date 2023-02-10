using Microsoft.AspNetCore.Mvc;
using Web.Apps.Chat.Services;

namespace Web.Apps.Chat.API.CrispAutoSuggestProxy;

[Route("/Chat/CrispAutoSuggestProxy/PhraseCheck")]
public class PhraseCheckController : Controller
{
    private readonly ChatHandler _chatHandler;

    public PhraseCheckController(ChatHandler chatHandler) => _chatHandler = chatHandler;

    [HttpGet]
    public IActionResult GetPhraseCheck([FromQuery(Name = "message")] string message) => Ok(message);
}
