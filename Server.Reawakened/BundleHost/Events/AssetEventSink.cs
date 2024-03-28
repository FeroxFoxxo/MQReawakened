using Server.Base.Core.Abstractions;
using Server.Reawakened.BundleHost.Events.Arguments;

namespace Server.Reawakened.BundleHost.Events;

public class AssetEventSink : IEventSink
{
    public delegate void AssetBundlesLoadedEventHandler(AssetBundleLoadEventArgs @event);

    public void InvokeAssetBundlesLoaded(AssetBundleLoadEventArgs @event) => AssetBundlesLoaded?.Invoke(@event);

    public event AssetBundlesLoadedEventHandler AssetBundlesLoaded;
}
