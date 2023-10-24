using Server.Base.Core.Configs;
using Server.Base.Network.Services;
using System.Net;

namespace Server.Base.Network.Helpers;

public class IpLimiter(InternalRConfig internalServerConfig, NetStateHandler handler)
{
    private readonly Dictionary<IPAddress, IPAddress> _ipAddressTable = [];

    public IPAddress Intern(IPAddress ipAddress)
    {
        if (_ipAddressTable.TryGetValue(ipAddress, out var interned))
            return interned;

        interned = ipAddress;
        _ipAddressTable[ipAddress] = interned;

        return interned;
    }

    public bool Verify(IPAddress ourAddress)
    {
        var netStates = handler.Instances;

        var count = 0;

        foreach (var unused in netStates.Where(compState => ourAddress.Equals(compState.Address)))
        {
            ++count;

            if (count >= internalServerConfig.MaxAddresses)
                return false;
        }

        return true;
    }
}
