using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._e__EmailHandler;

public class GetMessageDetail : ExternalProtocol
{
    public override string ProtocolName => "eg";

    public ILogger<GetMessageDetail> Logger { get; set; }

    public override void Run(string[] message)
    {
        var messageId = int.Parse(message[5]);

        if (Player.Character.EmailMessages.Count > 0)
        {
            var mail = Player.Character.EmailMessages[0];

            mail.EmailHeaderModel.MessageId = messageId;

            Player.SendXt("eg", messageId, mail.ToString());
        }
        else
            Logger.LogError("Invalid messageId: {messageId}", messageId);
    }
}
