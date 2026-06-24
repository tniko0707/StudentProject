using Application.Repositories;
using Domain.Models;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<int> CalculateUserBookings(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<User> FindUserByLogin(string username)
        {
            return await _userRepository.FindByLoginAsync(username);
        }

        public async Task<bool> VerifyPassword(User user, string password)
        {
            return PasswordHasher.VerifyPassword(password, user.PasswordHash);
        }

        public async Task<User> RegistrateUser(string username, string password)
        {
            if (await _userRepository.FindByLoginAsync(username) != null)
            {
                throw new Exception("Пользователь с таким именем существует");
            }
            var user = new User
            {
                Login = username,
                Role = Role.Admin,
            };

            user.PasswordHash = PasswordHasher.HashPassword(password);

            await _userRepository.AddAsync(user);
            return user;
        }

        public async Task<User> FindByIdAsync(Guid userId, CancellationToken ct)
        {
            return await _userRepository.FindByIdAsync(userId, ct);
        }



    }
}

