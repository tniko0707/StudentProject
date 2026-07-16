using System.Security.Cryptography;
using System.Text;
using Users.Application.Services;

namespace Users.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public bool VerifyPassword(string providedPassword, string storedHash)
        {
            string hashedPassword = HashPassword(providedPassword);
            return hashedPassword.Equals(storedHash, StringComparison.Ordinal);
        }
    }
}
