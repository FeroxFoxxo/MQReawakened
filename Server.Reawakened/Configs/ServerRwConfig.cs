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
            { GameVersion.vLate2013, "10TNMT_3102_TVE" },
            { GameVersion.vEarly2013, "regnaRrewoP_2102_ORP" }
        };

        CurrentTimedEvent = new Dictionary<GameVersion, string>
        {
            { GameVersion.v2014, "tnevEytraPboBegnopS" },
            { GameVersion.vLate2013, string.Empty },
            { GameVersion.vEarly2013, string.Empty }
        };
    }
}
