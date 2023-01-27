namespace Server.Base.Accounts.Models;

public class AccountTag
{
    public string Name { get; set; }

    public string Value { get; set; }

    public AccountTag()
    {
    }

    public AccountTag(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
