using Users.Domain.Models;

namespace Users.Application.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetByLoginAsync(string login, CancellationToken ct = default);
        Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);
        Task<User> AddAsync(User user, CancellationToken ct = default);
    }
}
