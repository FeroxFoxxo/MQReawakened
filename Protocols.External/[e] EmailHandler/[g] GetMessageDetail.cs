using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._e__EmailHandler;
public class GetMessageDetail : ExternalProtocol
{
    public override string ProtocolName => "eg";

    public ILogger<GetMessageDetail> Logger { get; set; }

    public override void Run(string[] message)
    {
        var messageId = int.Parse(message[5]);

        if (messageId >= 0 && messageId <= Player.Character.EmailMessages.Count)
        {
            var mail = Player.Character.EmailMessages[messageId];

            Player.SendXt("eg", messageId, mail.ToString());
        }

        else
            Logger.LogError("Invalid messageId: {messageId}", messageId);
    }
}
