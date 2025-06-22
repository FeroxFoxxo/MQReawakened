namespace Server.Base.Core.Abstractions;

public interface IInjectModules
{
    IEnumerable<Module> Modules { get; set; }
}
