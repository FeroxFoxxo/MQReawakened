using Server.Base.Core.Configs;

namespace Server.Base.Core.Extensions;

public static class StrictFlags
{
    public static bool StrictNetworkCheck(this InternalRwConfig rwConfig) =>
        (rwConfig.NetworkType & (rwConfig.NetworkType - 1)) == 0;
}
