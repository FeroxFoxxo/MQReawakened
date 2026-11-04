using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._Z__PetHandler;
public class UpdatePetMode : ExternalProtocol
{
    public override string ProtocolName => "Zm";

    public PetAbilities PetAbilities { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ILogger<UpdatePetMode> Logger { get; set; }

    public override void Run(string[] message)
    {
        if (Player == null || !Player.Character.Pets.TryGetValue
            (Player.GetItemIdOfEquippedPet(), out var pet) || Player.Character.Pets.Count == 0 ||
            !PetAbilities.PetAbilityData.TryGetValue(int.Parse(Player.GetItemIdOfEquippedPet()), out var petAbilities))
        {
            Logger.LogInformation("{characterName} has no pet equipped!", Player.CharacterName);
            return;
        }
        
        // Enzo does not have a pet ability
        if (pet.AbilityParams.AbilityType == PetAbilityType.Invalid)
            return;

        var minimumEnergyRequired = pet.MaxEnergy / petAbilities.UseCount;

        if (pet.CurrentEnergy < minimumEnergyRequired)
        {
            Player.SendXt("Zo", Player.UserId);
            return;
        }

        if (pet.PetIsBusy(Player))
        {
            Logger.LogInformation("{characterName}'s pet isn't ready to use an ability!", Player.CharacterName);
            return;
        }

        if (petAbilities.IsAttackAbility() && petAbilities.GetPetDetectedEnemies(Player, ServerRConfig).Count <= 0)
        {
            Logger.LogInformation("{characterName}'s pet detected no enemies to attack!", Player.CharacterName);
            return;
        }

        if (petAbilities.AbilityType == PetAbilityType.Heal &&
            Player.Character.CurrentLife >= Player.Character.MaxLife)
        {
            Logger.LogInformation("{characterName}'s pet can't heal health at full HP!", Player.CharacterName);
            return;
        }

        Player.SendAbility(TimerThread, ServerRConfig);
    }
}
