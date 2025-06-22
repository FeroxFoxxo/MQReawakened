using Microsoft.Extensions.Logging;
using Server.Base.Network;

namespace Server.Base.Core.Models;

public interface INetStateData
{
    void RemovedState(NetState state, IServiceProvider services, ILogger logger);
}
