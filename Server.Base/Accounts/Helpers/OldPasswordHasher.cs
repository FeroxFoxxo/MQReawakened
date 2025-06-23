using Server.Base.Core.Extensions;
using Server.Base.Database.Accounts;
using System.Security.Cryptography;
using System.Text;

namespace Server.Base.Accounts.Helpers;

public class OldPasswordHasher
{
    private static readonly byte[] _hashBuffer = new byte[256];
    private static readonly SHA512 _sha512HashProvider = SHA512.Create();

    private static string HashSha512(string phrase)
    {
        var length = Encoding.ASCII.GetBytes(phrase, 0, phrase.Length > 256 ? 256 : phrase.Length, _hashBuffer, 0);
        var hashed = _sha512HashProvider.ComputeHash(_hashBuffer, 0, length);

        return BitConverter.ToString(hashed);
    }

    private static string GetPassword(string username, string password) =>
        HashSha512(username.Sanitize() + password);

    public static bool CheckPassword(AccountModel account, string plainPassword) =>
        account.Password == GetPassword(account.Username, plainPassword);
}
