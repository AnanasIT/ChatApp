namespace PasswordHelperModel;

using System.Security.Cryptography;
using System.Text;

public class HashPassword
{
    public static string PasswordHash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string hashPassword)
    {
        return hashPassword == PasswordHash(password);
    }
}