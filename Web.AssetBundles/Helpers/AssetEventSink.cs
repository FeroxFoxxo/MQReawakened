using Web.AssetBundles.Events;

namespace Web.AssetBundles.Helpers;

public class AssetEventSink
{
    public delegate void AssetBundlesLoadedEventHandler(AssetBundleLoadEventArgs @event);

    public void InvokeAssetBundlesLoaded(AssetBundleLoadEventArgs @event) => AssetBundlesLoaded?.Invoke(@event);

    public event AssetBundlesLoadedEventHandler AssetBundlesLoaded;
}
