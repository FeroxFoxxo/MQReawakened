using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Events;

namespace Server.Reawakened.Core.Services;
public class GetServerAddress(IServer server, IHostApplicationLifetime appLifetime, ServerRwConfig config, ReawakenedEventSink sink, ILogger<GetServerAddress> logger) : IService
{
    public string ServerAddress { get; set; }

    public void Initialize() => appLifetime.ApplicationStarted.Register(AppStarted);

    private void AppStarted()
    {
        ServerAddress = string.IsNullOrEmpty(config.DomainName) ? server.Features.Get<IServerAddressesFeature>().Addresses.First() : config.DomainName;
        logger.LogInformation("Set listening URL to: {Url}", ServerAddress);
        sink.InvokeServerAddressFound();
    }
}
