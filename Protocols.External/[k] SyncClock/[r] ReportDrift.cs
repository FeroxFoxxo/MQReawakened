using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._k__SyncClock;

public class ReportDrift : ExternalProtocol
{
    public override string ProtocolName => "kr";

    public ILogger<ReportDrift> Logger { get; set; }

    public override void Run(string[] message)
    {
        var error = long.Parse(message[5]);
        var drift = double.Parse(message[6]);
        var lag = float.Parse(message[7]);

        if (lag > 0 && Math.Abs(drift) > .01)
            Logger.LogDebug("Client {Client}: Drifting by {Drift:0.####}s. {Error} ticks out of sync. " +
                            "{Lag} effective lag.", NetState, drift, error, lag);
    }
}
