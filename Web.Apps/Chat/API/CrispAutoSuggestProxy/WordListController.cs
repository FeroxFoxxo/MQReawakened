using Microsoft.AspNetCore.Mvc;
using Web.Apps.Chat.Services;

namespace Web.Apps.Chat.API.CrispAutoSuggestProxy;

[Route("/Chat/CrispAutoSuggestProxy/WordList")]
public class WordListController(ChatHandler chatHandler) : Controller
{
    [HttpGet]
    public IActionResult GetWordsList() => File(chatHandler.EncryptedWordList, "application/octet-stream");
}
