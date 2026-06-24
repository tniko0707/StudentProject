using Domain.Models;

namespace Application.Repositories
{
    public interface IUserRepository
    {
        Task<User?> FindByLoginAsync(string login, CancellationToken ct = default);
        Task<User?> FindByIdAsync(Guid userId, CancellationToken ct = default);
        Task<User> AddAsync(User user, CancellationToken ct = default);
    }
}
