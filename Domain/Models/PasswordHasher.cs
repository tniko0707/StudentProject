using System.Security.Cryptography;
using System.Text;

namespace Domain.Models
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public static bool VerifyPassword(string providedPassword, string storedHash)
        {
            string hashedPassword = HashPassword(providedPassword);
            return hashedPassword.Equals(storedHash, StringComparison.Ordinal);
        }
    }
}
