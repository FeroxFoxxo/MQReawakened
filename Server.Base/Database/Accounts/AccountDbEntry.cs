using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Helpers;
using Server.Base.Accounts.Models;
using Server.Base.Core.Models;

namespace Server.Base.Database.Accounts;
public class AccountDbEntry : PersistantData
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public GameMode GameMode { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastLogin { get; set; }
    public string[] IpRestrictions { get; set; }
    public string[] LoginIPs { get; set; }
    public List<AccountTag> Tags { get; set; }
    public int Flags { get; set; }

    public AccountDbEntry() => InitializeList();

    public AccountDbEntry(string username, string password, string email, PasswordHasher hasher)
    {
        username = username.ToLower();
        email = email.ToLower();

        Username = username;
        Password = hasher.GetPassword(username, password);
        Email = email;

        AccessLevel = AccessLevel.Player;
        GameMode = GameMode.Default;
        Created = DateTime.UtcNow;
        LastLogin = DateTime.UtcNow;

        InitializeList();
    }

    public void InitializeList()
    {
        IpRestrictions = [];
        LoginIPs = [];
        Tags = [];
    }
}
