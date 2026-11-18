using Server.Reawakened.Network.Protocols;

namespace Protocols.External._a__ChatHandler;

public class RemoveFavorite : ExternalProtocol
{
    public override string ProtocolName => "ar";
    
    public override void Run(string[] message)
    {
        var slotId = int.Parse(message[5]);
        
        if (!Player.Character.ChatFavorites.ContainsKey(slotId))
            return;
        
        Player.Character.Write.ChatFavorites.Remove(slotId);
    }
}
