using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Events;

namespace Server.Reawakened.Core.Services;
public class GetServerAddress(IServer server, ServerRwConfig config, EventSink eSink, ReawakenedEventSink sink, ILogger<GetServerAddress> logger) : IService
{
    public string ServerAddress { get; set; }

    public void Initialize() => eSink.ServerHosted += AppStarted;

    private void AppStarted()
    {
        ServerAddress = string.IsNullOrEmpty(config.DomainName) ? server.Features.Get<IServerAddressesFeature>().Addresses.First() : config.DomainName;

        if (ServerAddress.Contains("0.0.0.0"))
            ServerAddress = ServerAddress.Replace("0.0.0.0", "localhost");

        logger.LogInformation("Set listening URL to: {Url}", ServerAddress);
        sink.InvokeServerAddressFound();
    }
}
