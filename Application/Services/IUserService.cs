using Domain.Models;

namespace Application.Services
{
    public interface IUserService
    {
        Task<User> RegistrateUser(string username, string password);
        Task<User> FindByIdAsync(Guid guid, CancellationToken ct = default);
        Task<User> FindUserByLogin(string username);
        Task<bool> VerifyPassword(User user, string password);
        Task<int> CalculateUserBookings(User user);
    }
}
