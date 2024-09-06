namespace Server.Base.Database.Abstractions;
public abstract class DbLock
{
    public object Lock = new();
}
