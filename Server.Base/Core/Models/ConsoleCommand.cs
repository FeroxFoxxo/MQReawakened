using Server.Base.Network.Enums;

namespace Server.Base.Core.Models;

public class ConsoleCommand
{
    public delegate void RunConsoleCommand(string[] command);

    public string Name { get; set; }
    public string Description { get; set; }
    public RunConsoleCommand CommandMethod { get; set; }
    public NetworkType NetworkType { get; set; }
}
