using Server.Base.Core.Abstractions;

namespace Web.Apps.Chat.Models;

public class ChatConfig : IRConfig
{
    public string CrispKey { get; }
    public string TerminationCharacter { get; }
    public string[] Words { get; }

    public ChatConfig()
    {
        CrispKey = "654472ea5fbeadc4da5f5de312ffae7e";
        TerminationCharacter = "\r\n";
        Words = new[]
        {
            "Example",
            "Test"
        };
    }
}
