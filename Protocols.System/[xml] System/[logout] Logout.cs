using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Pets;
using System.Xml;

namespace Protocols.System._xml__System;

public class Logout : SystemProtocol
{
    public override string ProtocolName => "logout";

    public PlayerContainer PlayerContainer { get; set; }
    public ILogger<Logout> Logger { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        if (Player != null)
        {
            PetModel pet = null;
            Player?.Character?.Pets.TryGetValue(Player.GetItemIdOfEquippedPet(), out pet);

            if (Player.GetItemIdOfEquippedPet() != "0" && pet != null)
                pet.LogoutAndDespawnPet(Player);

            lock (PlayerContainer.Lock)
            {
                if (Player.Character != null)
                    foreach (var player in PlayerContainer.GetPlayersByFriend(Player.CharacterId))
                        player?.SendXt("fz", Player.CharacterName);
            }

            Player?.Remove(Logger);
        }

        SendXml("logout", string.Empty);
    }
}
