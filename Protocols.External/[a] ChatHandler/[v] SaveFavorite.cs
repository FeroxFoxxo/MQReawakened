using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Models.Protocol;

namespace Protocols.External._a__ChatHandler;

public class SaveFavorite : ExternalProtocol
{
    public override string ProtocolName => "av";
    
    public override void Run(string[] message)
    {
        var sentenceId = int.Parse(message[5]);
        var specificId = int.Parse(message[6]);

        if (Player.Character.ChatFavorites == null)
            Player.Character.Write.ChatFavorites = [];
        
        if (Player.Character.ChatFavorites != null && Player.Character.ChatFavorites.Count < 9)
        {
            var slotId = 0;
            while (Player.Character.ChatFavorites.ContainsKey(slotId))
                slotId++;

            var sentenceModel = new SentenceModel
            {
                SentenceTextId = sentenceId,
                SpecificTextId = specificId
            };
        
            Player.Character.Write.ChatFavorites.TryAdd(slotId, sentenceModel);
        
            SendXt("av", "1", slotId, sentenceModel.ToString());
        }
        else
        {
            SendXt("av", "0", "-2");
        }
    }
}
