using Server.Base.Core.Abstractions;
using Web.AssetBundles.Events.Arguments;

namespace Web.AssetBundles.Events;

public class AssetEventSink : IEventSink
{
    public delegate void AssetBundlesLoadedEventHandler(AssetBundleLoadEventArgs @event);

    public void InvokeAssetBundlesLoaded(AssetBundleLoadEventArgs @event) => AssetBundlesLoaded?.Invoke(@event);

    public event AssetBundlesLoadedEventHandler AssetBundlesLoaded;
}
