namespace Server.Base.Logging;

public class NoopDisposable : IDisposable
{
    public void Dispose() => GC.SuppressFinalize(this);
}
