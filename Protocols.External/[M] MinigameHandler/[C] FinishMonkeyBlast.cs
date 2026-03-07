using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Internal;
using UnityEngine;

namespace Protocols.External._M__MinigameHandler;
public class FinishMonkeyBlast : ExternalProtocol
{
    public override string ProtocolName => "MC";

    public ServerRConfig ServerRConfig { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<FinishMonkeyBlast> Logger { get; set; }

    public override void Run(string[] message)
    {
        var stars = (short)int.Parse(message[5]);
        var boosts = int.Parse(message[6]);
        var bounces = int.Parse(message[7]);
        var perfectLaunches = int.Parse(message[8]);

        Player.AddReputation(GetXpReward(stars), ServerRConfig);
        Player.AddBananas(GetBananaReward(stars), InternalAchievement, Logger);

        Player.SendCashUpdate();
    }

    private float GetBananaReward(short stars)
    {
        var level = Mathf.Clamp(Player.Character.GlobalLevel, 1, 35);
        var multiplier = new float[4] { 0.5f, 1f, 1.5f, 2f };
        var bananas = 0.25f * (level * multiplier[stars]);
        return bananas;
    }

    private int GetXpReward(short stars)
    {
        var reputationNeeded = Mathf.Clamp(GetXpNeededToLevel(), 1, 34000);
        var multiplier = new float[4] { 0.01f, 0.013f, 0.02f, 0.04f };
        var reputation = 0.25f * (reputationNeeded * multiplier[stars]);
        return (int)reputation;
    }

    private int GetXpNeededToLevel()
    {
        var character = Player.Character;
        if (character.GlobalLevel >= 65)
            return 1;
        var reputationNeeded = character.ReputationForNextLevel - character.ReputationForCurrentLevel;
        return (reputationNeeded < 1) ? 1 : reputationNeeded;
    }
}
