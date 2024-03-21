using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._b__LocalizationHandler;
public class SetLocale : ExternalProtocol
{
    public override string ProtocolName => "bs";

    public override void Run(string[] message)
    {
        var locale = int.Parse(message[5]);

        Player.TempData.Locale = locale;

        Player.SendXt("bs", locale);
    }
}
