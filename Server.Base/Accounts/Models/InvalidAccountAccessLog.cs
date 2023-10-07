using System.Net;

namespace Server.Base.Accounts.Models;

public class InvalidAccountAccessLog(IPAddress address)
{
    public IPAddress Address { get; set; } = address;

    public int Counts { get; set; }

    public DateTime LastAccessTime { get; set; } = DateTime.UtcNow;

    public bool HasExpired => DateTime.UtcNow >= LastAccessTime + TimeSpan.FromHours(1.0);
}
