using System.Security.Cryptography;
using System.Text;
using BusinessLayer.Interface;

namespace BusinessLayer.Service;

public class PasswordService : IPasswordService
{
    private const int Iterations = 100_000;
    private const int HashSize = 32;
    public (byte[] hash, byte[] salt) Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        
        var salt = RandomNumberGenerator.GetBytes(32);
        var hash = Rfc2898DeriveBytes
            .Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return (hash, salt);
    }

    public bool Verify(string password, byte[]? hash, byte[]? salt)
    {
        if (string.IsNullOrEmpty(password) || hash == null || salt == null || hash.Length != HashSize)
        {
            return false;
        }
        var computed = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return CryptographicOperations.FixedTimeEquals(computed, hash);
    }
}