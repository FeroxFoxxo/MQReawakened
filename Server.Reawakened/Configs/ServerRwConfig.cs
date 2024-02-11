using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Configs;

public class ServerRwConfig : IRwConfig
{
    public Dictionary<GameVersion, string> CurrentEvent { get; set; }
    public Dictionary<GameVersion, string> CurrentTimedEvent { get; set; }

    public ServerRwConfig()
    {
        CurrentEvent = new Dictionary<GameVersion, string>
        {
            { GameVersion.v2014, "boBegnopS_4102_TVE" },
            { GameVersion.v2013, "10TNMT_3102_TVE" }
        };

        CurrentTimedEvent = new Dictionary<GameVersion, string>
        {
            { GameVersion.v2014, "tnevEytraPboBegnopS" },
            { GameVersion.v2013, string.Empty }
        };
    }
}
