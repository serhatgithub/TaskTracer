using System.Security.Cryptography;
using System.Text;

namespace TaskTracer.Api.Services;

public class PasswordService
{
    public byte[] Hash(string password, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public bool Verify(string plainPassword, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA512(storedSalt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(plainPassword));
        return computedHash.SequenceEqual(storedHash);
    }
    public void CreateHash(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    }

    
}
