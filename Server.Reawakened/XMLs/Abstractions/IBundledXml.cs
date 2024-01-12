using Microsoft.Extensions.Logging;

namespace Server.Reawakened.XMLs.Abstractions;

public interface IBundledXml<T> : IInternalBundledXml
{
    public ILogger<T> Logger { get; set; }
}
