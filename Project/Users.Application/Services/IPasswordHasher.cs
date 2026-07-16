namespace Users.Application.Services
{
    public interface IPasswordHasher
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string providedPassword, string storedHash);
    }
}