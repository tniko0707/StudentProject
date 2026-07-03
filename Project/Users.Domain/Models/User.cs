using System.ComponentModel.DataAnnotations;

namespace Users.Domain.Models
{
    public class User
    {
        public User()
        {
            
        }

        public User(string login, string email, string passwordHash, Role role)
        {
            UserId = Guid.NewGuid();
            Login = login;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = DateTime.UtcNow;
        }
        public Guid UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
