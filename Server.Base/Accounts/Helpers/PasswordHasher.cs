using Server.Base.Database.Accounts;
using System.Security.Cryptography;

namespace Server.Base.Accounts.Helpers;

public class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA512;

    public static bool CheckPassword(AccountModel account, string plainPassword)
    {
        if (InternalCheckPassword(account, plainPassword))
            return true;

        if (OldPasswordHasher.CheckPassword(account, plainPassword))
        {
            account.Write.Password = GetPassword(plainPassword);
            return true;
        }

        return false;
    }

    private static bool InternalCheckPassword(AccountModel account, string plainPassword)
    {
        if (string.IsNullOrEmpty(account?.Password) || string.IsNullOrEmpty(plainPassword))
            return false;

        try
        {
            var hashWithSaltBytes = Convert.FromBase64String(account.Password);

            var salt = new byte[SaltSize];
            Array.Copy(hashWithSaltBytes, 0, salt, 0, SaltSize);

            var storedHash = new byte[HashSize];
            Array.Copy(hashWithSaltBytes, SaltSize, storedHash, 0, HashSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(plainPassword, salt, Iterations, _hashAlgorithmName);
            var passwordHash = pbkdf2.GetBytes(HashSize);

            return CryptographicOperations.FixedTimeEquals(passwordHash, storedHash);
        }
        catch (Exception)
        {
           return false;
        }
    }

    public static string GetPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, _hashAlgorithmName);
        var hash = pbkdf2.GetBytes(HashSize);

        var hashWithSaltBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashWithSaltBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashWithSaltBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashWithSaltBytes);
    }
}
