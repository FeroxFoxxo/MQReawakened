using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using System.Xml;

namespace Protocols.System._xml__System;

public class Logout : SystemProtocol
{
    public override string ProtocolName => "logout";

    public ServerRConfig ServerRConfig { get; set; }
    public ILogger<Logout> Logger { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        if (Player != null)
        {
            if (Player.Character != null)
            {
                if (Player.Character.Pets.TryGetValue(Player.GetEquippedPetId(ServerRConfig), out var pet))
                {
                    Player.TempData.PetEnergyRegenTimer?.Stop();
                    pet.LastTimePetWasEquipped = DateTime.Now;
                    pet.HasGainedOfflineEnergy = false;
                }
                if (Player.Character.CurrentLife >= 0)
                {
                    Player.Character.Write.CurrentLife = Player.Character.MaxLife;
                }
            }

            Player.Remove(Logger);
        }

        SendXml("logout", string.Empty);
    }
}
