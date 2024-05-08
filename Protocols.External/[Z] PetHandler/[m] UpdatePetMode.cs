using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._Z__PetHandler;
public class UpdatePetMode : ExternalProtocol
{
    public override string ProtocolName => "Zm";

    public TimerThread TimerThread { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<UpdatePetMode> Logger { get; set; }

    public override void Run(string[] message)
    {
        if (Player == null || !Player.Character.Pets.TryGetValue
            (Player.GetEquippedPetId(ServerRConfig), out var pet) || !Player.Character.Pets.Any())
        {
            Logger.LogInformation("{characterName} has no pet equipped!", Player.CharacterName);
            return;
        }

        if (pet.CurrentEnergy <= 0)
        {
            Player.SendXt("Zo", Player.UserId);
            return;
        }

        if (pet.InCoopState() || pet.PetAbilityModel.AbilityCooldown > Player.Room.Time && pet.PetAbilityModel.AbilityCooldown != 0)
        {
            Logger.LogInformation("{characterName}'s pet isn't ready to use an ability!", Player.CharacterName);
            return;
        }

        if (pet.PetAbilityModel.IsAttackAbility() && pet.PetAbilityModel.GetDetectedEnemies(Player).Count <= 0)
        {
            Logger.LogInformation("{characterName}'s pet detected no enemies to attack!", Player.CharacterName);
            return;
        }

        if (pet.PetAbilityModel.PetAbilityParams.AbilityType == PetAbilityType.Heal &&
            Player.Character.Data.CurrentLife >= Player.Character.Data.MaxLife)
        {
            Logger.LogInformation("{characterName}'s pet can't heal health at full HP!", Player.CharacterName);
            return;
        }

        pet.PetAbilityModel.SendAbility(Player, pet, ItemCatalog, TimerThread);
    }
}
